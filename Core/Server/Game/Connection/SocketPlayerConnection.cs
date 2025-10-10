using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.Entrance.ClientSession.Templates;
using Vint.Core.Logging;
using Vint.Core.Server.Game.Protocol.Codecs.Buffer;
using Vint.Core.Server.Game.Protocol.Codecs.Impl;
using Vint.Core.Server.Game.Protocol.Commands;
using Vint.Core.User.Components;

namespace Vint.Core.Server.Game.Connection;

public class SocketPlayerConnection(
    int id,
    IServiceScope serviceScope,
    Socket socket
) : PlayerConnection(id, serviceScope.ServiceProvider) {
    public IPEndPoint EndPoint { get; } = (IPEndPoint)socket.RemoteEndPoint!;

    public override bool IsLoggedIn => IsConnected && IsSocketConnected && ClientSession != null! && UserContainer != null! && Player != null!;
    bool IsSocketConnected => Socket.Connected;
    bool IsConnected { get; set; }

    Socket Socket { get; } = socket;
    Protocol.Protocol Protocol { get; } = serviceScope.ServiceProvider.GetRequiredService<Protocol.Protocol>();
    GameServer Server { get; } = serviceScope.ServiceProvider.GetRequiredService<GameServer>();

    public override async Task SetUsername(string username) {
        await base.SetUsername(username);
        Logger = Logger.WithPlayer(this);
    }

    public override async Task Kick(string? reason) {
        Logger.Warning("Player kicked (reason: '{Reason}')", reason);
        await Disconnect();
    }

    public override async Task OnConnected() {
        Logger = Logger.WithEndPoint(EndPoint);

        ClientSession = new ClientSessionTemplate().Create();
        Logger.Information("New socket connected ({Id})", Id);

        _ = Task.Run(ReceiveAndExecute);

        await Send(new InitTimeCommand(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        await Share(ClientSession);

        IsConnected = true;
    }

    public override async Task Send(ICommand command) {
        if (!IsSocketConnected)
            return;

        try {
            if (Logger.IsEnabled(LogEventLevel.Verbose))
                Logger.Verbose("Encoding {Command}", command);

            await using ProtocolBuffer buffer = new(this);

            Protocol
                .GetCodec(new TypeCodecInfo(typeof(ICommand)))
                .Encode(buffer, command);

            using MemoryStream stream = new();
            await using BinaryWriter writer = new BigEndianBinaryWriter(stream);
            buffer.Wrap(writer);

            byte[] bytes = stream.ToArray();
            await Socket.SendAsync(bytes);

            if (Logger.IsEnabled(LogEventLevel.Verbose))
                Logger.Verbose("Sent {Command}: {Size} bytes ({Hex})", command, bytes.Length, Convert.ToHexString(bytes));
        } catch (Exception e) {
            Logger.Error(e, "Failed to send {Command}", command);
        }
    }

    async Task Disconnect() {
        if (!IsConnected) return;

        try {
            Socket.Shutdown(SocketShutdown.Both);
        } finally {
            Socket.Close();
            await OnDisconnected();
        }
    }

    async Task OnDisconnected() {
        if (!IsConnected) return;

        IsConnected = false;
        Logger.Information("Socket disconnected");

        try {
            if (UserContainer != null!) {
                await UserContainer.RemoveConnection(this);
                await UserContainer.Entity.RemoveComponent<UserOnlineComponent>();

                if (InSquad)
                    await Squad!.RemoveMember(UserContainer.Id);
            }

            if (InLobby) {
                LobbyBase lobby = LobbyPlayer!.Lobby;

                if (LobbyPlayer.InRound) {
                    Round round = LobbyPlayer.Round;
                    await round.RemoveTanker(LobbyPlayer.Tanker);
                }

                await lobby.RemovePlayer(LobbyPlayer);
            }

            if (Spectating) {
                Round round = Spectator!.Round;
                await round.RemoveSpectator(Spectator);
            }
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while disconnecting socket");
        } finally {
            Server.RemovePlayer(Id);

            foreach (IEntity entity in SharedEntities)
                entity.SharedPlayers.TryRemove(this);

            SharedEntities.Clear();
        }

        await DisposeAsync();
    }

    public override async Task Tick(CancellationToken cancellationToken = default) {
        if (!IsSocketConnected) {
            await Kick("Zombie");
            return;
        }

        await base.Tick(cancellationToken);
    }

    async Task ReceiveAndExecute() {
        if (!IsSocketConnected)
            return;

        try {
            await using NetworkStream stream = new(Socket, FileAccess.Read);
            using BinaryReader reader = new BigEndianBinaryReader(stream);

            while (true) {
                await using ProtocolBuffer buffer = ProtocolBuffer.Unwrap(reader, this);
                long availableForRead = buffer.Stream.Length - buffer.Stream.Position;

                while (availableForRead > 0) {
                    Logger.Verbose("Decode buffer bytes available: {Count}", availableForRead);

                    IServerCommand command = (IServerCommand)Protocol
                        .GetCodec(new TypeCodecInfo(typeof(ICommand)))
                        .Decode(buffer);

                    try {
                        await command.Execute(this, ServiceProvider);
                    } catch (Exception e) {
                        Logger.Error(e, "Failed to execute {Command}", command);
                    }

                    availableForRead = buffer.Stream.Length - buffer.Stream.Position;
                }
            }
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while reading socket");
            await Disconnect();
            throw;
        }
    }

    public override void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public override async ValueTask DisposeAsync() {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    void Dispose(bool disposing) {
        if (disposing) {
            Socket.Dispose();
            DelayedTasks.Clear();
            SharedEntities.Clear();
            serviceScope.Dispose();

            ClientSession.SharedPlayers.TryRemove(this);
            ClientSession.Dispose();
        }
    }

    async ValueTask DisposeAsyncCore() {
        Socket.Dispose();
        DelayedTasks.Clear();
        SharedEntities.Clear();

        if (serviceScope is IAsyncDisposable ad)
            await ad.DisposeAsync();
        else serviceScope.Dispose();

        ClientSession.SharedPlayers.TryRemove(this);
        ClientSession.Dispose();
    }

    ~SocketPlayerConnection() => Dispose(false);
}

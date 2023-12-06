using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using NetCoreServer;
using Serilog;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Entrance;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Commands;
using Vint.Utils;

namespace Vint.Core.Server;

public class PlayerConnection(TcpServer server, Protocol.Protocol protocol) : TcpSession(server) {
    public ILogger Logger { get; private set; } = Log.Logger.ForType(typeof(PlayerConnection));
    public IEntity ClientSession { get; private set; } = null!;

    BlockingCollection<byte[]> ReceivedPackets { get; } = new();
    BlockingCollection<ICommand> SendingCommands { get; } = new();

    protected override void OnConnecting() =>
        Logger = Logger.WithPlayer(this);

    protected override void OnConnected() {
        ClientSession = new ClientSessionTemplate().Create();

        Logger.Information("New socket connected");

        new Thread(ReceiveLoop) { Name = $"Player {ClientSession.Id} receive loop" }.Start();
        new Thread(SendLoop) { Name = $"Player {ClientSession.Id} send loop" }.Start();

        Send(new InitTimeCommand(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        ClientSession.Share(this);
    }

    protected override void OnDisconnected() {
        ReceivedPackets.CompleteAdding();
        SendingCommands.CompleteAdding();

        ReceivedPackets.Dispose();
        SendingCommands.Dispose();

        Logger.Information("Socket disconnected");
    }

    protected override void OnError(SocketError error) =>
        Logger.Error("Socket caught an error: {Error}", error);

    protected override void OnReceived(byte[] bytes, long offset, long size) {
        Logger.Verbose("Received {Size} bytes ({Hex})", size, Convert.ToHexString(bytes[..(int)size]));
        ReceivedPackets.Add(bytes);
    }

    public void Send(ICommand command) {
        Logger.Debug("Enqueuing {Command} for sending", command);
        SendingCommands.Add(command);
    }

    void ReceiveLoop() {
        try {
            while (true) {
                byte[] packet = ReceivedPackets.Take();

                ProtocolBuffer buffer = new(new OptionalMap());
                MemoryStream stream = new(packet);
                BinaryReader reader = new BigEndianBinaryReader(stream);

                if (!buffer.Unwrap(reader))
                    throw new InvalidDataException("Failed to unwrap packet");

                long availableForRead = buffer.Stream.Length - buffer.Stream.Position;

                while (availableForRead > 0) {
                    Logger.Verbose("Decode buffer bytes available: {Available}", availableForRead);

                    ICommand command = (ICommand)protocol.GetCodec(new TypeCodecInfo(typeof(ICommand))).Decode(buffer);
                    Logger.Debug("Received {Command}", command);

                    availableForRead = buffer.Stream.Length - buffer.Stream.Position;

                    try {
                        command.Execute(this);
                    } catch (Exception e) {
                        Logger.Error(e, "Failed to execute {Command}", command);
                    }
                }
            }
        } catch (Exception e) {
            Logger.Error(e, "Socket caught an exception in the receive loop");
            Disconnect();
        }
    }

    void SendLoop() {
        try {
            while (true) {
                ICommand command = SendingCommands.Take();

                ProtocolBuffer buffer = new(new OptionalMap());

                protocol.GetCodec(new TypeCodecInfo(typeof(ICommand))).Encode(buffer, command);

                using MemoryStream stream = new();
                using BinaryWriter writer = new BigEndianBinaryWriter(stream);

                buffer.Wrap(writer);

                byte[] bytes = stream.ToArray();

                SendAsync(bytes);

                Logger.Verbose("Sent {Command}: {Size} bytes ({Hex})", command, bytes.Length, Convert.ToHexString(bytes));
            }
        } catch (Exception e) {
            Logger.Error(e, "Socket caught an exception in the send loop");
        }
    }

    public override string ToString() => $"PlayerConnection {{ " +
                                         $"Id: {ClientSession?.Id}; " +
                                         //$"Username: {Player.Username}; " + //todo: username
                                         $"Endpoint: {Socket.RemoteEndPoint /*as IPEndPoint*/} }}";
}

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using NetCoreServer;
using Serilog;
using Vint.Core.Database;
using Vint.Core.ECS;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.ECS.Events.Entrance.Login;
using Vint.Core.ECS.Events.Entrance.Registration;
using Vint.Core.ECS.Templates.Entrance;
using Vint.Core.ECS.Templates.User;
using Vint.Core.Protocol.Codecs.Buffer;
using Vint.Core.Protocol.Codecs.Impl;
using Vint.Core.Protocol.Commands;
using Vint.Core.Utils;

namespace Vint.Core.Server;

public interface IPlayerConnection {
    public ILogger Logger { get; }

    public Player Player { get; set; }
    public IEntity User { get; }
    public IEntity ClientSession { get; }

    public void Register(
        string username,
        string encryptedPasswordDigest,
        string email,
        string hardwareFingerprint,
        bool subscribed,
        bool steam,
        bool quickRegistration);

    public void Login(
        bool rememberMe,
        string hardwareFingerprint);

    public void ChangePassword(string passwordDigest);

    public void Send(ICommand command);

    public void Send(IEvent @event);

    public void Share(IEntity entity);

    public void Share(params IEntity[] entities);

    public void Share(IEnumerable<IEntity> entities);
}

public class PlayerConnection(TcpServer server, Protocol.Protocol protocol) : TcpSession(server), IPlayerConnection {
    BlockingCollection<byte[]> ReceivedPackets { get; } = new();
    BlockingCollection<ICommand> SendingCommands { get; } = new();
    DatabaseContext Database { get; } = new();
    public ILogger Logger { get; private set; } = Log.Logger.ForType(typeof(PlayerConnection));

    public Player Player {
        get => _player;
        set {
            _player = value;
            Database.Players.Attach(_player);
        }
    }

    public IEntity User { get; private set; } = null!;
    public IEntity ClientSession { get; private set; } = null!;

    public void Register(
        string username,
        string encryptedPasswordDigest,
        string email,
        string hardwareFingerprint,
        bool subscribed,
        bool steam,
        bool quickRegistration) {
        Logger.Information("Registering player '{Username}'", username);

        if (username == "fail") {
            Send(new RegistrationFailedEvent());
            return;
        }

        Player = new Player(Logger, username, email) {
            CountryCode = IpUtils.GetCountryCode((Socket.RemoteEndPoint as IPEndPoint)!.Address) ?? "US",
            HardwareFingerprint = hardwareFingerprint,
            Subscribed = subscribed,
            RegistrationTime = DateTimeOffset.UtcNow
        };

        ChangePassword(encryptedPasswordDigest);

        Database.Players.Add(Player);
        Database.Save();

        Login(true, hardwareFingerprint);
    }

    public void Login(
        bool rememberMe,
        string hardwareFingerprint) {
        Player.LastLoginTime = DateTimeOffset.UtcNow;
        Player.HardwareFingerprint = hardwareFingerprint;

        if (rememberMe) {
            Encryption encryption = new();

            byte[] autoLoginToken = new byte[32];
            new Random().NextBytes(autoLoginToken);

            byte[] encryptedAutoLoginToken = encryption.EncryptAutoLoginToken(autoLoginToken, Player.PasswordHash);

            Player.AutoLoginToken = autoLoginToken;

            Send(new SaveAutoLoginTokenEvent(Player.Username, encryptedAutoLoginToken));
        }

        User = new UserTemplate().Create(Player);
        Share(User);

        ClientSession.AddComponent(User.GetComponent<UserGroupComponent>());

        Logger.Warning("'{Username}' logged in", Player.Username);

        Database.Save();
    }

    public void ChangePassword(string passwordDigest) {
        Encryption encryption = new();

        byte[] passwordHash = encryption.RsaDecrypt(Convert.FromBase64String(passwordDigest));
        Player.PasswordHash = passwordHash;

        Database.Save();
    }

    public void Send(ICommand command) {
        Logger.Debug("Enqueuing {Command}", command);

        SendingCommands.Add(command);
    }

    public void Send(IEvent @event) => ClientSession.Send(@event);

    public void Share(IEntity entity) => entity.Share(this);

    public void Share(params IEntity[] entities) => entities.ToList().ForEach(Share);

    public void Share(IEnumerable<IEntity> entities) => entities.ToList().ForEach(Share);

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
        } finally {
            Logger.Warning("Receive loop ended");

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
        } finally {
            Logger.Warning("Send loop ended");

            Disconnect();
        }
    }

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public override string ToString() => $"PlayerConnection {{ " +
                                         $"ClientSession Id: '{ClientSession?.Id}'; " +
                                         $"Username: '{Player?.Username}'; " +
                                         $"Endpoint: {Socket.RemoteEndPoint} }}";

    Player _player = null!;
}

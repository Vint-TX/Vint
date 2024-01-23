using System.Net;
using System.Net.Sockets;
using NetCoreServer;
using Serilog;
using Vint.Core.Battles;
using Vint.Core.ChatCommands;
using Vint.Core.ECS.Events.Ping;
using Vint.Core.Utils;

namespace Vint.Core.Server;

public class GameServer(
    IPAddress host,
    ushort port
) : TcpServer(host, port) {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(GameServer));
    Protocol.Protocol Protocol { get; } = new();

    public IBattleProcessor BattleProcessor { get; private set; } = null!;
    public IMatchmakingProcessor MatchmakingProcessor { get; private set; } = null!;

    public List<IPlayerConnection> PlayerConnections { get; } = [];

    protected override PlayerConnection CreateSession() => new(this, Protocol);

    protected override void OnConnected(TcpSession session) => PlayerConnections.Add((PlayerConnection)session);

    protected override void OnDisconnected(TcpSession session) => PlayerConnections.Remove((PlayerConnection)session);

    protected override void OnStarted() {
        Logger.Information("Started");

        BattleProcessor = new BattleProcessor();
        MatchmakingProcessor = new MatchmakingProcessor(BattleProcessor);

        new Thread(() => MatchmakingProcessor.StartTicking()) { Name = "Matchmaking ticker" }.Start();
        new Thread(() => BattleProcessor.StartTicking()) { Name = "Battle ticker" }.Start();
        new Thread(PingLoop) { Name = "Ping loop" }.Start();
        
        ChatCommandProcessor.RegisterCommands();
    }

    protected override void OnError(SocketError error) => Logger.Error("Server caught an error: {Error}", error);

    void PingLoop() {
        while (true) {
            if (!IsStarted) return;

            foreach (IPlayerConnection playerConnection in PlayerConnections.ToArray()) {
                try {
                    playerConnection.Send(new PingEvent(DateTimeOffset.UtcNow));
                } catch (Exception e) {
                    Logger.Error(e, "Socket caught an exception while sending ping event");
                }
            }

            Thread.Sleep(10000);
        }
    }
}
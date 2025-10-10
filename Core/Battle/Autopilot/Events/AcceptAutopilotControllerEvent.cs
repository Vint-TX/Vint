using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Autopilot.Events;

[ProtocolId(1450950143213)]
public class AcceptAutopilotControllerEvent : IServerEvent {
    [ProtocolName("Version")] public int BotId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.LobbyPlayer?.Tanker is not HumanTanker tanker)
            return;

        BotTanker? bot = tanker.Round.BotTankers.FirstOrDefault(bot => bot.BotId == BotId);
        if (bot == null) return;

        await bot.OnControllerFound(tanker);
    }
}

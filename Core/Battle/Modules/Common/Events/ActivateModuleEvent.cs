using Vint.Core.Battle.Common.Events;
using Vint.Core.Battle.Modules.Impl.Base;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Events;

[ProtocolId(1486015564167)]
public class ActivateModuleEvent : TimeEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        Round round = tanker?.Round!;

        if (tanker == null || round.Properties.DisabledModules)
            return;

        IEntity slot = entities.Single();
        BattleModule? module = tanker.Tank.Modules.FirstOrDefault(module => module.SlotEntity == slot);

        if (module == null)
            return;

        await module.Activate();
    }
}

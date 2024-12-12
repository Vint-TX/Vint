using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Module;

[ProtocolId(1486015564167)]
public class ActivateModuleEvent : TimeEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.Battle.Properties.DisabledModules)
            return;

        IEntity slot = entities.Single();
        BattleTank tank = connection.BattlePlayer.Tank!;
        BattleModule? module = tank.Modules.SingleOrDefault(module => module.SlotEntity == slot);

        if (module == null)
            return;

        await module.Activate();
    }
}

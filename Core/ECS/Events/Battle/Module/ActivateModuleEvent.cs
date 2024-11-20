using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Battle.Module;

[ProtocolId(1486015564167)]
public class ActivateModuleEvent : TimeEvent, IServerEvent {
    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.Battle.Properties.DisabledModules) return Task.CompletedTask;

        IEntity slot = entities.Single();
        BattleTank tank = connection.BattlePlayer.Tank!;
        BattleModule? module = tank.Modules.SingleOrDefault(module => module.SlotEntity == slot);

        module?.Activate();
        return Task.CompletedTask;
    }
}

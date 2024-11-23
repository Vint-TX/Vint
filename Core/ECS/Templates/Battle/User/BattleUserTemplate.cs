using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.User;

[ProtocolId(-2043703779834243389)]
public class BattleUserTemplate : EntityTemplate {
    IEntity Create(IEntity user, IEntity battle) => Entity("battle/battleuser",
        builder => builder
            .AddComponent<BattleUserComponent>()
            .AddGroupComponent<UserGroupComponent>(user)
            .AddGroupComponent<BattleGroupComponent>(battle));

    public IEntity CreateAsTank(IEntity user, IEntity battle, IEntity? team) {
        IEntity entity = Create(user, battle);

        if (team != null)
            entity.AddGroupComponent<TeamGroupComponent>(team);

        entity.AddComponent<UserInBattleAsTankComponent>();
        entity.AddComponent(new BattleUserInventoryCooldownSpeedComponent(1));
        return entity;
    }

    public IEntity CreateAsSpectator(IEntity user, IEntity battle) {
        IEntity entity = Create(user, battle);

        entity.AddComponent<UserInBattleAsSpectatorComponent>();
        return entity;
    }
}

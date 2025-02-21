using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Modules.Inventory;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.User;

[ProtocolId(-2043703779834243389)]
public class BattleUserTemplate : EntityTemplate {
    IEntity Create(IEntity user, IEntity round) => Entity("battle/battleuser",
        builder => builder
            .AddComponent<BattleUserComponent>()
            .AddGroupComponent<UserGroupComponent>(user)
            .AddGroupComponent<BattleGroupComponent>(round));

    public IEntity CreateAsTank(IEntity user, IEntity round, IEntity? team) {
        IEntity entity = Create(user, round);

        if (team != null)
            entity.AddGroupComponent<TeamGroupComponent>(team);

        entity.AddComponent<UserInBattleAsTankComponent>();
        entity.AddComponent(new BattleUserInventoryCooldownSpeedComponent(1));
        return entity;
    }

    public IEntity CreateAsSpectator(IEntity user, IEntity round) {
        IEntity entity = Create(user, round);

        entity.AddComponent<UserInBattleAsSpectatorComponent>();
        return entity;
    }
}

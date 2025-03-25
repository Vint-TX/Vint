using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Modules.Common.Components.Inventory;
using Vint.Core.Battle.Player.User.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Player.User.Templates;

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

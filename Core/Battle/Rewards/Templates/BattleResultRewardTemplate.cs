using Vint.Core.Battle.Rewards.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rewards.Templates;

[ProtocolId(1513235522063)]
public abstract class BattleResultRewardTemplate : EntityTemplate {
    protected IEntity Create(string configPath) => Entity(configPath,
        builder => builder
            .AddComponent<PersonalBattleRewardComponent>()
            .AddGroupComponent<BattleRewardGroupComponent>());
}

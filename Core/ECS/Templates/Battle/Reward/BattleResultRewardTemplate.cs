using Vint.Core.ECS.Components.Battle.Rewards;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Reward;

[ProtocolId(1513235522063)]
public abstract class BattleResultRewardTemplate : EntityTemplate {
    protected IEntity Create(string configPath) {
        IEntity entity = Entity(configPath, builder => builder.AddComponent(new PersonalBattleRewardComponent()));

        entity.AddComponent(new BattleRewardGroupComponent(entity));
        return entity;
    }
}
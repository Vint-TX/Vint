using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Effect.Type;
using Vint.Core.ECS.Components.Battle.Unit;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Effect;

[ProtocolId(1485335642293)]
public class DroneEffectTemplate : EffectBaseTemplate {
    public IEntity Create(BattlePlayer battlePlayer, TimeSpan duration, IEntity weapon, float targetingDistance) {
        const string configPath = "battle/effect/drone";

        IEntity entity = Create(configPath, battlePlayer, duration, true, false);
        DroneMoveConfigComponent droneMoveConfigComponent = ConfigManager.GetComponent<DroneMoveConfigComponent>(configPath);

        entity.AddComponent(droneMoveConfigComponent);
        entity.AddComponent(new UnitMoveComponent(battlePlayer.Tank!.Position + droneMoveConfigComponent.SpawnPosition));
        entity.AddComponent(new UnitTargetingConfigComponent(targetingDistance));
        entity.AddComponent<DroneEffectComponent>();
        entity.AddComponent<UnitComponent>();
        entity.AddComponentFrom<UserGroupComponent>(battlePlayer.BattleUser);
        entity.AddGroupComponent<UnitGroupComponent>(weapon);
        return entity;
    }
}

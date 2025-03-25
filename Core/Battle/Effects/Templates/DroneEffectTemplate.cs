using Vint.Core.Battle.Effects.Components.Impl;
using Vint.Core.Battle.Modules.Unit.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Effects.Templates;

[ProtocolId(1485335642293)]
public class DroneEffectTemplate : EffectBaseTemplate {
    public IEntity Create(Tanker tanker, TimeSpan duration, IEntity weapon, float targetingDistance) {
        const string configPath = "battle/effect/drone";

        IEntity entity = Create(configPath, tanker, duration, true, false);
        DroneMoveConfigComponent droneMoveConfigComponent = ConfigManager.GetComponent<DroneMoveConfigComponent>(configPath);
        BattleTank tank = tanker.Tank;

        entity.AddComponent(droneMoveConfigComponent);
        entity.AddComponent(new UnitMoveComponent(tank.Position + droneMoveConfigComponent.SpawnPosition, tank.Orientation));
        entity.AddComponent(new UnitTargetingConfigComponent(targetingDistance));
        entity.AddComponent<DroneEffectComponent>();
        entity.AddComponent<UnitComponent>();
        entity.AddComponentFrom<UserGroupComponent>(tanker.BattleUser);
        entity.AddGroupComponent<UnitGroupComponent>(weapon);
        return entity;
    }
}

using System.Numerics;
using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Bonus.Events;

[ProtocolId(-4179984519411113540)]
public class BonusTakingRequestEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity tankEntity = entities[1];

        if (!tankEntity.HasComponent<TankComponent>() ||
            connection.LobbyPlayer?.Tanker is not HumanTanker human)
            return;

        BotTanker? bot = null;

        if (!tankEntity.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            if (tankEntity != human.Tank.Entities.Tank)
                return;
        } else if (!human.ControlledBots.TryGetValue(autopilotComponent.Id, out bot))
            return;

        Round round = human.Round;
        IBonusProcessor? bonusProcessor = round.BonusProcessor;

        if (bonusProcessor == null)
            return;

        IEntity bonus = entities[0];
        BattleTank tank = bot?.Tank ?? human.Tank;
        BonusBox? bonusBox = bonusProcessor.FindByEntity(bonus);

        if (bonusBox?.StateManager.CurrentState is not Spawned spawned) return;

        float bonusHeight = CalculateHeight(bonusBox.SpawnPosition.Y,
            bonusBox.RegionPosition.Y,
            bonusBox.ConfigComponent.FallSpeed,
            spawned.SpawnTime);

        Vector3 tankPosition = tank.Position;
        Vector3 bonusPosition = bonusBox.RegionPosition with { Y = bonusHeight };

        if (Vector3.Distance(tankPosition, bonusPosition) > 10) return; // broken??

        await bonusProcessor.Take(bonusBox, tank);
    }

    static float CalculateHeight(float spawnHeight, float regionHeight, float fallSpeed, DateTimeOffset spawnTime) {
        float maxFallDuration = Math.Abs((spawnHeight - regionHeight) / fallSpeed);
        float currentFallDuration = (float)Math.Clamp((DateTimeOffset.UtcNow - spawnTime).TotalSeconds, 0, maxFallDuration);

        return MathUtils.Map(currentFallDuration, 0, maxFallDuration, spawnHeight, regionHeight);
    }
}

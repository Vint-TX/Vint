using System.Numerics;
using Vint.Core.Battle.Bonus;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Bonus;

[ProtocolId(-4179984519411113540)]
public class BonusTakingRequestEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        IBonusProcessor? bonusProcessor = tanker?.Round.BonusProcessor;

        if (tanker == null || bonusProcessor == null)
            return;

        BattleTank battleTank = tanker.Tank;
        IEntity bonus = entities.First();
        BonusBox? bonusBox = bonusProcessor.FindByEntity(bonus);

        if (bonusBox?.StateManager.CurrentState is not Spawned spawned) return;

        float bonusHeight = CalculateHeight(bonusBox.SpawnPosition.Y,
            bonusBox.RegionPosition.Y,
            bonusBox.ConfigComponent.FallSpeed,
            spawned.SpawnTime);

        Vector3 tankPosition = battleTank.Position;
        Vector3 bonusPosition = bonusBox.RegionPosition with { Y = bonusHeight };

        if (Vector3.Distance(tankPosition, bonusPosition) > 10) return; // broken??

        await bonusProcessor.Take(bonusBox, battleTank);
    }

    static float CalculateHeight(float spawnHeight, float regionHeight, float fallSpeed, DateTimeOffset spawnTime) {
        float maxFallDuration = Math.Abs((spawnHeight - regionHeight) / fallSpeed);
        float currentFallDuration = (float)Math.Clamp((DateTimeOffset.UtcNow - spawnTime).TotalSeconds, 0, maxFallDuration);

        return MathUtils.Map(currentFallDuration, 0, maxFallDuration, spawnHeight, regionHeight);
    }
}

using System.Numerics;
using Vint.Core.Battles.Bonus;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Bonus;

[ProtocolId(-4179984519411113540)]
public class BonusTakingRequestEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        BattlePlayer? battlePlayer = connection.BattlePlayer;
        IBonusProcessor? bonusProcessor = battlePlayer?.Battle.BonusProcessor;

        if (battlePlayer is not { InBattleAsTank: true } || bonusProcessor == null) return;

        BattleTank battleTank = battlePlayer.Tank!;
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

namespace Vint.Core.Battle.Weapons;

public interface IStreamWeaponHandler : IWeaponHandler {
    float DamagePerSecond { get; }

    TimeSpan GetTimeSinceLastHit(long incarnationId);
}

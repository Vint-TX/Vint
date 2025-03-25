namespace Vint.Core.Battle.Weapons.Handlers;

public interface IStreamWeaponHandler : IWeaponHandler {
    float DamagePerSecond { get; }

    TimeSpan GetTimeSinceLastHit(long incarnationId);
}

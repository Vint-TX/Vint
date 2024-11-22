namespace Vint.Core.Battles.Weapons;

public interface IStreamWeaponHandler : IWeaponHandler {
    float DamagePerSecond { get; }

    TimeSpan GetTimeSinceLastHit(long incarnationId);
}

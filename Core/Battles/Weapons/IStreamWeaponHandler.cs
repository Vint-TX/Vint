namespace Vint.Core.Battles.Weapons;

public interface IStreamWeaponHandler : IWeaponHandler {
    public float DamagePerSecond { get; }

    public TimeSpan GetTimeSinceLastHit(long incarnationId);
}

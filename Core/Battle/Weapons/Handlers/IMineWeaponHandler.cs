namespace Vint.Core.Battle.Weapons;

public interface IMineWeaponHandler : ISplashWeaponHandler {
    Task Explode();
}

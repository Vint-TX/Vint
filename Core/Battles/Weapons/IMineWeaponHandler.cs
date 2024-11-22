namespace Vint.Core.Battles.Weapons;

public interface IMineWeaponHandler : ISplashWeaponHandler {
    Task Explode();
}

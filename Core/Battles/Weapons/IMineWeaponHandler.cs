namespace Vint.Core.Battles.Weapons;

public interface IMineWeaponHandler : ISplashWeaponHandler {
    public Task Explode();
}

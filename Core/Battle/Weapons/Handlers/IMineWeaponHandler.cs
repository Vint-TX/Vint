namespace Vint.Core.Battle.Weapons.Handlers;

public interface IMineWeaponHandler : ISplashWeaponHandler {
    Task Explode();
}

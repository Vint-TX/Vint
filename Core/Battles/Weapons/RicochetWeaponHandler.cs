using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Weapons;

public class RicochetWeaponHandler(
    BattleTank battleTank
) : DiscreteWeaponHandler(battleTank);
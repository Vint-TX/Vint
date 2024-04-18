using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Modules.Interfaces;

public interface IKillModule {
    public void OnKill(BattleTank target);
}
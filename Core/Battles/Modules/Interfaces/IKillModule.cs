using Vint.Core.Battles.Tank;

namespace Vint.Core.Battles.Modules.Interfaces;

public interface IKillModule {
    Task OnKill(BattleTank target);
}

using Vint.Core.Battles.Tank;

namespace Vint.Core.Battles.Modules.Interfaces;

public interface IKillModule {
    public Task OnKill(BattleTank target);
}

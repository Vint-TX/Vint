using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Modules.Interfaces;

public interface IKillModule {
    Task OnKill(BattleTank target);
}

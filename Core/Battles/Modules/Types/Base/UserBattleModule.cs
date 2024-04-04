using Vint.Core.Battles.Effects;

namespace Vint.Core.Battles.Modules.Types.Base;

public abstract class UserBattleModule : BattleModule {
    public abstract Effect GetEffect();
}
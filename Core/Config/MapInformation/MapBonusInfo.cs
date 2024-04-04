using Vint.Core.Battles;

namespace Vint.Core.Config.MapInformation;

public readonly record struct MapBonusInfo(
    BonusList Deathmatch,
    BonusList? TeamDeathmatch,
    BonusList? CaptureTheFlag
) {
    public BonusList Get(BattleMode battleMode) => battleMode switch {
        BattleMode.DM => Deathmatch,
        BattleMode.TDM => TeamDeathmatch ?? CaptureTheFlag!.Value,
        BattleMode.CTF => CaptureTheFlag ?? TeamDeathmatch!.Value,
        _ => throw new ArgumentException(null, nameof(battleMode))
    };
}
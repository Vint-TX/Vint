using Vint.Core.Battles;

namespace Vint.Core.Config.MapInformation;

public class MapBonusInfo {
    public BonusList Deathmatch { get; set; } = null!;
    public BonusList? TeamDeathmatch { get; set; } = null!;
    public BonusList? CaptureTheFlag { get; set; } = null!;

    public BonusList Get(BattleMode battleMode) => battleMode switch {
        BattleMode.DM => Deathmatch,
        BattleMode.TDM => TeamDeathmatch ?? CaptureTheFlag!,
        BattleMode.CTF => CaptureTheFlag ?? TeamDeathmatch!,
        _ => throw new ArgumentException(null, nameof(battleMode))
    };
}
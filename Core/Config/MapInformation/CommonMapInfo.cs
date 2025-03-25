using Vint.Core.Battle.Mode;

namespace Vint.Core.Config.MapInformation;

public readonly record struct CommonMapInfo(
    GoldMapInfo Gold,
    Dictionary<BattleMode, double> ModesProbability
);

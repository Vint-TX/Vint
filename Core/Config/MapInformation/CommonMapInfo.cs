using Vint.Core.ECS.Enums;

namespace Vint.Core.Config.MapInformation;

public readonly record struct CommonMapInfo(
    GoldMapInfo Gold,
    Dictionary<BattleMode, double> ModesProbability
);

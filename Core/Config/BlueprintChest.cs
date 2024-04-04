namespace Vint.Core.Config;

public readonly record struct BlueprintChest(
    string Name,
    int MinAmount,
    int MaxAmount,
    int MinTier2Amount,
    int MaxTier2Amount,
    int MinTier3Amount,
    int MaxTier3Amount,
    float Tier1Probability,
    float Tier2Probability,
    float Tier3Probability
);
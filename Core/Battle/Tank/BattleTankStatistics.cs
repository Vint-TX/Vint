namespace Vint.Core.Battle.Tank;

public class BattleTankStatistics {
    public int MaxKillStrike { get; set; }
    public int KillAssists { get; set; }
    public int Flags { get; set; }
    public int FlagAssists { get; set; }
    public int FlagReturns { get; set; }
    public int BonusesTaken { get; set; }
    public float DealtDamage { get; set; }
    public float TakenDamage { get; set; }

    public void Reset() {
        MaxKillStrike = 0;
        KillAssists = 0;
        Flags = 0;
        FlagAssists = 0;
        FlagReturns = 0;
        BonusesTaken = 0;
        DealtDamage = 0;
        TakenDamage = 0;
    }
}

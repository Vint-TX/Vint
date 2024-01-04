using SharpCompress;
using Vint.Core.Battles.Player;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Enums;
using Vint.Core.Server;

namespace Vint.Core.Battles.Mode;

public class DMHandler : ModeHandler {
    public DMHandler(Battle battle) : base(battle) =>
        SpawnPoints = Battle.MapInfo.SpawnPoints.Deathmatch.ToReadOnly();

    public ReadOnlyCollection<SpawnPoint> SpawnPoints { get; private set; }

    public override BattleMode BattleMode => BattleMode.DM;

    public override void Tick() { }

    public override BattlePlayer SetupBattlePlayer(IPlayerConnection player) {
        BattlePlayer tankPlayer = new(player, Battle, null, false);

        player.User.AddComponent(new TeamColorComponent(TeamColor.None));
        return tankPlayer;
    }

    public override void RemoveBattlePlayer(BattlePlayer player) {
        player.PlayerConnection.User.RemoveComponent<TeamColorComponent>();
    }

    public override void PlayerEntered(BattlePlayer player) { }

    public override void PlayerExited(BattlePlayer player) { }
}
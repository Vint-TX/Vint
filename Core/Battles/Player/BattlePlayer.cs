using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.User;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Player;

public class BattlePlayer {
    public BattlePlayer(IPlayerConnection playerConnection, Battle battle, IEntity? team, bool isSpectator) {
        PlayerConnection = playerConnection;
        Team = team;
        Battle = battle;
        IsSpectator = isSpectator;

        BattleUserTemplate battleUserTemplate = new();

        BattleUser = IsSpectator
                         ? battleUserTemplate.CreateAsSpectator(PlayerConnection.User, Battle.BattleEntity)
                         : battleUserTemplate.CreateAsTank(PlayerConnection.User, Battle.BattleEntity, Team);
    }

    public IPlayerConnection PlayerConnection { get; }
    public IEntity? Team { get; set; }
    public IEntity BattleUser { get; }

    public Battle Battle { get; }
    public BattleTank? Tank { get; set; }

    public bool IsSpectator { get; }
    public bool InBattleAsTank => Tank != null;
    public bool InBattle { get; set; }
    public bool IsPaused { get; set; }
    public bool IsKicked { get; set; }

    public DateTimeOffset BattleJoinTime { get; set; } = DateTimeOffset.UtcNow.AddSeconds(10);
    public DateTimeOffset? KickTime { get; set; }

    public void Init() {
        PlayerConnection.Share(Battle.BattleEntity, Battle.RoundEntity, Battle.BattleChatEntity);

        // todo modules & supplies

        PlayerConnection.User.AddComponent(Battle.BattleEntity.GetComponent<BattleGroupComponent>());
        Battle.ModeHandler.PlayerEntered(this);

        if (IsSpectator) {
            PlayerConnection.Share(BattleUser);
            InBattle = true;
        } else {
            Tank = new BattleTank(this);

            // todo modules

            InBattle = true;

            foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
                player.PlayerConnection.Share(Tank.Entities); // Share this player entities to players in battle

            Battle.ModeHandler.SortScoreTable();
        }

        PlayerConnection.Share(Battle.Players
            .Where(player => player != this && player.InBattleAsTank)
            .SelectMany(player => player.Tank!.Entities));
    }

    public void Tick() => Tank?.Tick();

    public override int GetHashCode() => PlayerConnection.GetHashCode();
}
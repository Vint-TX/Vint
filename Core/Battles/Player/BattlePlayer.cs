using Vint.Core.ECS.Components.Battle.Team;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Battle.User;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Player;

public class BattlePlayer {
    IEntity? _team;
    TeamColor _teamColor = TeamColor.None;

    public BattlePlayer(IPlayerConnection playerConnection, Battle battle, IEntity? team, bool isSpectator) {
        PlayerConnection = playerConnection;
        Team = team;
        Battle = battle;
        IsSpectator = isSpectator;

        if (IsSpectator)
            BattleUser = new BattleUserTemplate().CreateAsSpectator(PlayerConnection.User, Battle.Entity);
    }

    public IPlayerConnection PlayerConnection { get; }

    public IEntity? Team {
        get => _team;
        set {
            _team = value;
            TeamColor = _team?.GetComponent<TeamColorComponent>().TeamColor ?? TeamColor.None;
        }
    }

    public IEntity BattleUser { get; set; } = null!;

    public Battle Battle { get; }
    public BattleTank? Tank { get; set; }

    public TeamColor TeamColor {
        get => _teamColor;
        private set {
            _teamColor = value;

            if (PlayerConnection.User.HasComponent<TeamColorComponent>())
                PlayerConnection.User.ChangeComponent<TeamColorComponent>(component => component.TeamColor = _teamColor);
            else
                PlayerConnection.User.AddComponent(new TeamColorComponent(_teamColor));
        }
    }

    public bool IsSpectator { get; }
    public bool InBattleAsTank => Tank != null;
    public bool InBattle { get; set; }
    public bool IsPaused { get; set; }
    public bool IsKicked { get; set; }

    public DateTimeOffset BattleJoinTime { get; set; } = DateTimeOffset.UtcNow.AddSeconds(10);
    public DateTimeOffset? KickTime { get; set; }

    public void Init() {
        PlayerConnection.Share(Battle.Entity, Battle.RoundEntity, Battle.BattleChatEntity);

        // todo modules & supplies

        PlayerConnection.User.AddComponent(Battle.Entity.GetComponent<BattleGroupComponent>());
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

            Battle.ModeHandler.SortPlayers();
        }

        PlayerConnection.Share(Battle.Players
            .Where(player => player != this && player.InBattleAsTank)
            .SelectMany(player => player.Tank!.Entities));
    }

    public void Tick() => Tank?.Tick();

    public override int GetHashCode() => PlayerConnection.GetHashCode();
}
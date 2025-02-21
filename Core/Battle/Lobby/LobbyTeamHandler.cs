using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Battle;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Lobby;

public class LobbyTeamHandler(
    LobbyBase lobby
) {
    LobbyBase Lobby { get; } = lobby;
    BattleProperties Properties => Lobby.Properties;

    int TeamLimit { get; set; }

    IEntity? RedTeam { get; set; }
    IEntity? BlueTeam { get; set; }

    public FrozenDictionary<TeamColor, IEntity?> ColorToEntity { get; private set; } = null!;

    [MemberNotNullWhen(true, nameof(RedTeam), nameof(BlueTeam), nameof(RedPlayers), nameof(BluePlayers))]
    public bool IsTeamLobby => Properties.BattleMode.IsTeamMode();

    public IEnumerable<LobbyPlayer>? RedPlayers => IsTeamLobby ? Lobby.Players.Where(player => player.TeamColor == TeamColor.Red) : null;
    public IEnumerable<LobbyPlayer>? BluePlayers => IsTeamLobby ? Lobby.Players.Where(player => player.TeamColor == TeamColor.Blue) : null;

    public void Init() {
        CreateTeams();
        TeamLimit = Lobby.Entity.GetComponent<UserLimitComponent>().TeamLimit;
    }

    public async Task LobbyUpdated(ClientBattleParams oldParams, ClientBattleParams newParams) {
        CreateTeams();
        TeamLimit = Lobby.Entity.GetComponent<UserLimitComponent>().TeamLimit;

        Func<LobbyPlayer, IEntity?> teamSelector = oldParams.BattleMode.IsTeamMode() == newParams.BattleMode.IsTeamMode()
            ? player => ColorToEntity[player.TeamColor]
            : CalculateTeamFor;

        foreach (LobbyPlayer player in Lobby.Players)
            await player.SetTeam(teamSelector(player));
    }

    public async Task TrySwitchTeam(LobbyPlayer player) {
        if (!IsTeamLobby) return;

        TeamColor newColor = GetOppositeTeamColor(player.TeamColor);
        IEntity newTeam = ColorToEntity[newColor]!;

        int newTeamPlayersCount = newColor switch {
            TeamColor.Red => RedPlayers.Count(),
            TeamColor.Blue => BluePlayers.Count(),
            _ => throw new ArgumentOutOfRangeException(null)
        };

        if (newTeamPlayersCount >= TeamLimit)
            return;

        await player.SetTeam(newTeam);
    }

    public async Task ChooseAndSetTeamFor(LobbyPlayer player) {
        IEntity? team = CalculateTeamFor(player);
        await player.SetTeam(team);
    }

    public IEntity? GetOppositeTeamFor(LobbyPlayer player) =>
        GetOppositeTeam(player.Team);

    public IEntity? GetOppositeTeam(IEntity? team) {
        if (!IsTeamLobby)
            throw new InvalidOperationException("Not a team lobby");

        if (team == null) return null;
        if (team == RedTeam) return BlueTeam;
        if (team == BlueTeam) return RedTeam;

        throw new ArgumentOutOfRangeException(nameof(team), team, "Invalid team");
    }

    public static TeamColor GetOppositeTeamColor(TeamColor color) => color switch {
        TeamColor.None => TeamColor.None,
        TeamColor.Red => TeamColor.Blue,
        TeamColor.Blue => TeamColor.Red,
        _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Invalid color")
    };

    IEntity? CalculateTeamFor(LobbyPlayer player) { // todo balance
        if (!IsTeamLobby)
            return null;

        IEntity team = BluePlayers.Count() > RedPlayers.Count()
            ? RedTeam
            : BlueTeam;

        return team;
    }

    void CreateTeams() { // todo dispose entities
        if (IsTeamLobby) {
            RedTeam = new TeamTemplate().Create(TeamColor.Red);
            BlueTeam = new TeamTemplate().Create(TeamColor.Blue);
        } else {
            RedTeam = null;
            BlueTeam = null;
        }

        ColorToEntity = new Dictionary<TeamColor, IEntity?> {
            { TeamColor.Red, RedTeam },
            { TeamColor.Blue, BlueTeam },
            { TeamColor.None, null }
        }.ToFrozenDictionary();
    }
}

using Vint.Core.Config;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Squads.Components;

namespace Vint.Core.Squads;

public static class SquadUtils {
    static SquadConfigComponent Config { get; } = ConfigManager.GetComponent<SquadConfigComponent>("/squad");

    public static bool CanJoinSquad(IPlayerConnection connection) =>
        connection is { InLobby: false, Spectating: false } &&
        connection.Player.Rank >= Config.RankRestriction;
}

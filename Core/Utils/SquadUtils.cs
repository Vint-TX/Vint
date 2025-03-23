using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Squad;
using Vint.Core.Server.Game;

namespace Vint.Core.Utils;

public static class SquadUtils {
    static SquadConfigComponent Config { get; } = ConfigManager.GetComponent<SquadConfigComponent>("/squad");

    public static bool CanJoinSquad(IPlayerConnection connection) =>
        connection is { InLobby: false, Spectating: false } &&
        connection.Player.Rank >= Config.RankRestriction;
}

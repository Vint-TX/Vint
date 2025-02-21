using Vint.Core.Battle.Matchmaking;
using Vint.Core.Quests;

namespace Vint.Core.Battle.Lobby.Impl.Arcade;

public abstract class ArcadeLobby(
    QuestManager questManager
) : MatchmakingLobby(questManager) {
    public abstract ArcadeModeType ArcadeType { get; }
}

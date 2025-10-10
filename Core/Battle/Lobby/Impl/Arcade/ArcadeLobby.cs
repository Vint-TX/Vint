using Vint.Core.Battle.Autopilot;
using Vint.Core.Matchmaking;
using Vint.Core.Quests;

namespace Vint.Core.Battle.Lobby.Impl.Arcade;

public abstract class ArcadeLobby(
    QuestManager questManager,
    BotBuilder botBuilder
) : MatchmakingLobby(questManager, botBuilder) {
    public abstract ArcadeModeType ArcadeType { get; }
}

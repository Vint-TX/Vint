using Vint.Core.Battle.Autopilot;
using Vint.Core.Battle.Lobby.Templates;
using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.Quests;

namespace Vint.Core.Battle.Lobby.Impl;

public class RatingLobby(
    BattleProperties properties,
    QuestManager questManager,
    BotBuilder botBuilder
) : MatchmakingLobby(questManager, botBuilder) {
    public override BattleProperties Properties { get; protected set; } = properties;
    public override IEntity Entity { get; } = new MatchMakingLobbyTemplate().Create(properties);
}

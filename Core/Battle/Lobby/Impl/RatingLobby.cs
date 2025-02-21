using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Lobby;
using Vint.Core.Quests;

namespace Vint.Core.Battle.Lobby.Impl;

public class RatingLobby(
    BattleProperties properties,
    QuestManager questManager
) : MatchmakingLobby(questManager) {
    public override BattleProperties Properties { get; protected set; } = properties;
    public override IEntity Entity { get; } = new MatchMakingLobbyTemplate().Create(properties);
}

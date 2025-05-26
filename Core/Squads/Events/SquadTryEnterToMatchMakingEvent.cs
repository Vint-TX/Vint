using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Matchmaking;
using Vint.Core.Matchmaking.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1510144894187)]
public class SquadTryEnterToMatchMakingEvent(
    RatingMatchmakingProcessor rating,
    ArcadeMatchmakingProcessor arcade
) : IServerEvent {
    static IEnumerable<IEntity> Modes { get; } = GlobalEntities.GetEntities("matchmakingModes").ToList();

    public long MatchMakingModeId { get; private set; }
    public bool RatingMatchMakingMode { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InSquad || connection.InLobby) return;

        Squad squad = connection.Squad;
        if (squad.Leader != connection) return;

        IEntity? selectedMode = Modes.FirstOrDefault(mode => mode.Id == MatchMakingModeId);
        if (selectedMode == null) return;

        string[] configPathParts = selectedMode.TemplateAccessor!.ConfigPath!.Split('/');

        if (configPathParts[1] == "arcade") {
            if (!Enum.TryParse(configPathParts[3], true, out ArcadeModeType mode))
                return;

            await squad.Members.Send<EnteredToMatchmakingEvent>(selectedMode);
            await arcade.EnqueueSquad(squad, mode);
        } else {
            await squad.Members.Send<EnteredToMatchmakingEvent>(selectedMode);
            await rating.EnqueueSquad(squad);
        }
    }
}

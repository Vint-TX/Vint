using Vint.Core.Battle.Flags;
using Vint.Core.Battle.Mode.Team.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Flag;

[ProtocolId(-1910863908782544246)]
public class FlagDropRequestEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        Round round = tanker?.Round!;

        if (tanker == null || round.ModeHandler is not CTFHandler ctf)
            return;

        Core.Battle.Flags.Flag? flag = ctf.Flags.Values.SingleOrDefault(flag => flag.Entity == entities[0]);

        if (flag?.StateManager.CurrentState is not Captured captured || captured.Carrier != tanker)
            return;

        await captured.Drop(true);
    }
}

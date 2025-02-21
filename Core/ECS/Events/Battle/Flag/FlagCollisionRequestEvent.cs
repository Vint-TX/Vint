using System.Numerics;
using Vint.Core.Battle.Flags;
using Vint.Core.Battle.Mode.Team.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Flag;

[ProtocolId(1463741053998)]
public class FlagCollisionRequestEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        Round round = tanker?.Round!;

        if (tanker?.Tank.StateManager.CurrentState is not Active ||
            round.StateManager.CurrentState is not Running ||
            round.ModeHandler is not CTFHandler ctf) return;

        IEntity flagEntity = entities[1];
        Core.Battle.Flags.Flag flag = ctf.Flags.Values.Single(flag => flag.Entity == flagEntity);

        TeamColor tankTeamColor = tanker.TeamColor;
        TeamColor flagTeamColor = flag.TeamColor;
        bool isAllyFlag = tankTeamColor == flagTeamColor;

        switch (flag.StateManager.CurrentState) {
            case OnPedestal onPedestal: {
                if (Vector3.Distance(tanker.Tank.Position, flag.PedestalPosition) > 5) return;

                if (isAllyFlag) {
                    Core.Battle.Flags.Flag oppositeFlag = ctf.Flags.Values.Single(f => f != flag);
                    await TryDeliver(oppositeFlag, tanker);
                } else await onPedestal.Capture(tanker);

                break;
            }

            case OnGround onGround: {
                if (Vector3.Distance(tanker.Tank.Position, flag.Position) > 5) return;

                if (isAllyFlag) await onGround.Return(tanker);
                else await onGround.Pickup(tanker);

                break;
            }
        }
    }

    static async Task TryDeliver(Core.Battle.Flags.Flag flag, Tanker tanker) {
        if (flag.StateManager.CurrentState is not Captured captured || captured.Carrier != tanker)
            return;

        await captured.Deliver();
    }
}

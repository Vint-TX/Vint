using System.Numerics;
using Vint.Core.Battle.Autopilot.Components;
using Vint.Core.Battle.Flags.State;
using Vint.Core.Battle.Mode.Team;
using Vint.Core.Battle.Mode.Team.Impl;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Tank.State;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Flags.Events;

[ProtocolId(1463741053998)]
public class FlagCollisionRequestEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity tankEntity = entities[0];

        if (!tankEntity.HasComponent<TankComponent>() ||
            connection.LobbyPlayer?.Tanker is not HumanTanker human)
            return;

        BotTanker? bot = null;

        if (!tankEntity.TryGetComponent(out TankAutopilotComponent? autopilotComponent)) {
            if (tankEntity != human.Tank.Entities.Tank)
                return;
        } else if (!human.ControlledBots.TryGetValue(autopilotComponent.Id, out bot))
            return;

        Round round = human.Round;
        BattleTank tank = bot?.Tank ?? human.Tank;

        if (tank.StateManager.CurrentState is not Active ||
            round.StateManager.CurrentState is not Running ||
            round.ModeHandler is not CTFHandler ctf) return;

        IEntity flagEntity = entities[1];
        Flag flag = ctf.Flags.Values.First(flag => flag.Entity == flagEntity);

        TeamColor tankTeamColor = tank.Tanker.TeamColor;
        TeamColor flagTeamColor = flag.TeamColor;
        bool isAllyFlag = tankTeamColor == flagTeamColor;

        switch (flag.StateManager.CurrentState) {
            case OnPedestal onPedestal: {
                if (Vector3.Distance(tank.Position, flag.PedestalPosition) > 5) return;

                if (isAllyFlag) {
                    Flag oppositeFlag = ctf.Flags.Values.First(f => f != flag);
                    await TryDeliver(oppositeFlag, tank.Tanker);
                } else await onPedestal.Capture(tank.Tanker);

                break;
            }

            case OnGround onGround: {
                if (Vector3.Distance(tank.Position, flag.Position) > 5) return;

                if (isAllyFlag) await onGround.Return(tank.Tanker);
                else await onGround.Pickup(tank.Tanker);

                break;
            }
        }
    }

    static async Task TryDeliver(Flag flag, Tanker tanker) {
        if (flag.StateManager.CurrentState is not Captured captured || captured.Carrier != tanker)
            return;

        await captured.Deliver();
    }
}

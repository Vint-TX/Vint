using System.Numerics;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Battle.Movement;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Movement;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Movement;

[ProtocolId(6959116100408127452)]
public class MoveCommandEvent : IServerEvent {
    public MoveCommand MoveCommand { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        Round round = tanker?.Round!;

        if (tanker == null)
            return;

        BattleTank tank = tanker.Tank;
        IEntity tankEntity = tank.Entities.Tank;

        await round.Players
            .Where(player => player != tanker)
            .Send(new MoveCommandServerEvent(MoveCommand), tankEntity);

        await tankEntity.ChangeComponent<TankMovementComponent>(component => {
            MoveControl moveControl = new() {
                MoveAxis = MoveCommand.TankControlVertical ?? component.MoveControl.MoveAxis,
                TurnAxis = MoveCommand.TankControlHorizontal ?? component.MoveControl.TurnAxis
            };

            component.MoveControl = moveControl;

            if (MoveCommand.Movement.HasValue)
                component.Movement = MoveCommand.Movement.Value;

            if (MoveCommand.WeaponRotation.HasValue)
                component.WeaponRotation = MoveCommand.WeaponRotation.Value;

            if (MoveCommand.WeaponRotationControl.HasValue)
                component.WeaponControl = MoveCommand.WeaponRotationControl.Value;
        });

        if (!MoveCommand.Movement.HasValue ||
            tank.StateManager.CurrentState is Dead)
            return;

        ECS.Movement.Movement movement = MoveCommand.Movement.Value;

        // Remarks from https://github.com/Assasans/TXServer-Public/blob/database/TXServer/ECSSystem/Events/Battle/Movement/MoveCommandEvent.cs#L24
        // Calculate velocity based on 2 previous positions and last position is kept instead
        // Reasons:
        // - velocity sent by client may be corrupted by overflow
        // - latest position may be corrupted too
        Vector3 velocity = tank.Position - tank.PreviousPosition;

        tank.PreviousPosition = tank.Position;
        tank.Position = movement.Position;
        tank.Orientation = movement.Orientation;

        if (PhysicsUtils.IsOutsideMap(round.Properties.MapInfo.PuntativeGeoms, tank.Position, velocity, round.Properties.KillZoneEnabled)) {
            await tank.SelfDestruct();
            return;
        }

        round.MineProcessor.TryTriggerSingle(tank);
    }
}

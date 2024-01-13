using System.Numerics;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Movement;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Movement;

[ProtocolId(6959116100408127452)]
public class MoveCommandEvent : IServerEvent {
    public MoveCommand MoveCommand { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattleAsTank) return;

        IEntity tank = entities.Single();
        BattlePlayer battlePlayer = connection.BattlePlayer!;
        Battles.Battle battle = battlePlayer.Battle;

        MoveCommandServerEvent serverEvent = new(MoveCommand);

        foreach (IPlayerConnection playerConnection in battle.Players
                     .Where(player => player != battlePlayer)
                     .Select(player => player.PlayerConnection))
            playerConnection.Send(serverEvent, tank);


        BattleTank battleTank = battlePlayer.Tank!;
        if (!MoveCommand.Movement.HasValue || battleTank.StateManager.CurrentState is Dead) return; // ???

        ECS.Movement.Movement movement = MoveCommand.Movement.Value;

        // Remarks from https://github.com/Assasans/TXServer-Public/blob/database/TXServer/ECSSystem/Events/Battle/Movement/MoveCommandEvent.cs#L24
        // Calculate velocity based on 2 previous positions and last position is kept instead
        // Reasons:
        // - velocity sent by client may be corrupted by overflow
        // - latest position may be corrupted too

        Vector3 velocity = battleTank.Position - battleTank.PreviousPosition;

        battleTank.PreviousPosition = battleTank.Position;
        battleTank.Position = movement.Position;
        battleTank.Orientation = movement.Orientation;

        if (PhysicsUtils.CheckOverflow(movement.Position + velocity))
            battleTank.SelfDestructTime = DateTimeOffset.UtcNow;

        if (!battle.Properties.KillZoneEnabled) return;

        foreach (PuntativeGeometry geometry in battle.MapInfo.PuntativeGeoms)
            if (PhysicsUtils.IsInsideBox(movement.Position, geometry.Position, geometry.Size))
                battleTank.SelfDestructTime = DateTimeOffset.UtcNow;
    }
}
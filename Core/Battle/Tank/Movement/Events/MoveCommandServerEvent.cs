using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Movement.Events;

[ProtocolId(-4956413533647444536)]
public class MoveCommandServerEvent(
    MoveCommand moveCommand
) : IEvent {
    public MoveCommand MoveCommand { get; private set; } = moveCommand;
}

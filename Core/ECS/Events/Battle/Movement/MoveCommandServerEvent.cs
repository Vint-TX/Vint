using Vint.Core.ECS.Movement;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Movement;

[ProtocolId(-4956413533647444536)]
public class MoveCommandServerEvent(
    MoveCommand moveCommand
) : IEvent {
    public MoveCommand MoveCommand { get; private set; } = moveCommand;
}
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Autopilot.Events;

[ProtocolId(1450950144568)]
public class ChangeAutopilotControllerEvent : IEvent;

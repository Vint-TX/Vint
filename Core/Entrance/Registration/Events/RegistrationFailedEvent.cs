using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Registration.Events;

[ProtocolId(1438592306427)]
public class RegistrationFailedEvent : IEvent;
// Client-side b.ug: missing [JoinAll] in the signature of [RegistrationScreenSystem#UnlockScreenOnFail] (fixed)

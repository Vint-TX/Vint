using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Registration;

[ProtocolId(1438592306427)]
public class RegistrationFailedEvent : IEvent;
// Client-side b.ug: missing [JoinAll] in the signature of [RegistrationScreenSystem#UnlockScreenOnFail] (fixed)
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1439531278716)]
public class PersonalPasscodeEvent : IEvent {
    public static string Passcode => "j4xEgl7WRO9H7HwnK/R1c8FYws1jUdJorx2yoCB53Kw="; // hardcoded (mayb change in future)
}
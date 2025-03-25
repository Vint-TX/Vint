using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Login.Events;

[ProtocolId(1439531278716)]
public class PersonalPasscodeEvent : IEvent {
    public static string Passcode => "j4xEgl7WRO9H7HwnK/R1c8FYws1jUdJorx2yoCB53Kw="; // hardcoded (todo change in future)
}

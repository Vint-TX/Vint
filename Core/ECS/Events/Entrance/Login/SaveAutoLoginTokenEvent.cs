using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Login;

[ProtocolId(1438070264777)]
public class SaveAutoLoginTokenEvent(
    string username,
    byte[] token
) : IEvent {
    [ProtocolName("Uid")] public string Username => username;
    public byte[] Token => token;
}
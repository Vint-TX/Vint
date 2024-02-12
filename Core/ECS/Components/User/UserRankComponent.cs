using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(-1413405458500615976)]
public class UserRankComponent(
    int rank
) : IComponent {
    public int Rank { get; set; } = rank;
}
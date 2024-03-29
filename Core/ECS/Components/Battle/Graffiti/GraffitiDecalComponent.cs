using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Graffiti;

[ProtocolId(636100801609006236), ClientAddable, ClientRemovable]
public class GraffitiDecalComponent : IComponent {
    public Vector3 SprayPosition { get; private set; }
    public Vector3 SprayDirection { get; private set; }
    public Vector3 SprayUpDirection { get; private set; }
}
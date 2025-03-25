using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Vulcan;

[ProtocolId(-6843896944033144903), ClientAddable, ClientRemovable]
public class VulcanSlowDownComponent : IComponent {
    public bool IsAfterShooting { get; private set; }
    public int ClientTime { get; private set; }
}

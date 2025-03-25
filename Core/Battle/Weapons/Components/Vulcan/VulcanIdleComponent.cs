using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Vulcan;

[ProtocolId(-3791262141248621103), ClientAddable, ClientRemovable]
public class VulcanIdleComponent : IComponent {
    public int Time { get; private set; }
}

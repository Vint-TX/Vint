using Vint.Core.Config;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Leagues;

[ProtocolId(1508823738925)]
public class CurrentSeasonNumberComponent : IComponent {
    public int SeasonNumber { get; private set; } = (int)ConfigManager.ServerConfig.SeasonNumber;
}

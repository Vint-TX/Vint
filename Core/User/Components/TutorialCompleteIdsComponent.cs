using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Components;

[ProtocolId(1505286737090)]
public class TutorialCompleteIdsComponent : IComponent { // todo tutorial
    public List<long> CompletedIds { get; private set; } = [];
    public bool TutorialSkipped { get; private set; }
}

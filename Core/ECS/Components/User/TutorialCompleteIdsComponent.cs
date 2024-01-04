using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1505286737090)]
public class TutorialCompleteIdsComponent : IComponent { // todo tutorial
    public List<long> CompletedIds { get; private set; } = [];
    public bool TutorialSkipped { get; private set; }
}
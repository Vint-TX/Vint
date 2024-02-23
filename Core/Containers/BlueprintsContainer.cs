using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.Containers;

public class BlueprintsContainer(
    IEntity marketItem
) : Container(marketItem) {
    public override IEnumerable<IEntity> Open(IPlayerConnection connection, long amount) => throw new NotImplementedException();
}
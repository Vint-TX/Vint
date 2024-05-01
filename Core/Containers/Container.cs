using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.Containers;

public abstract class Container(
    IEntity marketItem
) {
    protected IEntity MarketItem { get; } = marketItem;

    public abstract IAsyncEnumerable<IEntity> Open(IPlayerConnection connection, long amount);
}

using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Templates;

public abstract class EntityTemplate {
    protected IEntity Entity(string? configPath, Action<IEntityBuilder> builder, bool temp = false) {
        EntityBuilder entityBuilder = new();

        entityBuilder.WithTemplateAccessor(this, configPath);
        builder(entityBuilder);

        return entityBuilder.Build(temp);
    }

    public static bool operator ==(EntityTemplate? x, EntityTemplate? y) => Equals(x, y);

    public static bool operator !=(EntityTemplate? x, EntityTemplate? y) => !(x == y);

    public override bool Equals(object? obj) {
        if (obj == null) return false;
        if (ReferenceEquals(this, obj)) return true;

        return GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode() =>
        unchecked((int)GetType().GetProtocolId().Id);
}

public abstract class MarketEntityTemplate : EntityTemplate {
    public abstract UserEntityTemplate UserTemplate { get; }
}

public abstract class UserEntityTemplate : EntityTemplate {
    public abstract MarketEntityTemplate MarketTemplate { get; }
}
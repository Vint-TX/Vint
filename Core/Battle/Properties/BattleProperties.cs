using Vint.Core.Battle.Mode;
using Vint.Core.Config;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Properties;

public class BattleProperties {
    public BattleProperties(BattleType type, ClientBattleParams clientParams) {
        SetValue(BattleProperty.Type, type);
        SetParams(clientParams);
    }

    BattleProperties() { }

    Dictionary<IBattleProperty, object> Properties { get; } = [];

    public ClientBattleParams GetParams() => new(
        GetValue(BattleProperty.BattleMode),
        GetValue(BattleProperty.Gravity),
        GetValue(BattleProperty.MapInfo).Id,
        GetValue(BattleProperty.FriendlyFire),
        GetValue(BattleProperty.KillZoneEnabled),
        GetValue(BattleProperty.DisabledModules),
        GetValue(BattleProperty.MaxPlayers),
        GetValue(BattleProperty.TimeLimit));

    public void SetParams(ClientBattleParams @params) {
        SetValue(BattleProperty.BattleMode, @params.BattleMode);
        SetValue(BattleProperty.Gravity, @params.Gravity);
        SetValue(BattleProperty.FriendlyFire, @params.FriendlyFire);
        SetValue(BattleProperty.KillZoneEnabled, @params.KillZoneEnabled);
        SetValue(BattleProperty.DisabledModules, @params.DisabledModules);
        SetValue(BattleProperty.MaxPlayers, @params.MaxPlayers);
        SetValue(BattleProperty.TimeLimit, @params.TimeLimit);

        SetValue(BattleProperty.MapInfo, ConfigManager.MapInfos.Single(info => info.Id == @params.MapId));
        SetValue(BattleProperty.MapEntity, ConfigManager.GetGlobalEntities("maps").Single(entity => entity.Id == @params.MapId));
    }

    public T GetValue<T>(IBattleProperty<T> property) where T : notnull =>
        Properties.TryGetValue(property, out object? value)
            ? (T)value
            : property.DefaultValue;

    public bool HasValue<T>(IBattleProperty<T> property) where T : notnull =>
        Properties.ContainsKey(property);

    public bool TryGetValue<T>(IBattleProperty<T> property, out T value) where T : notnull {
        if (Properties.TryGetValue(property, out object? obj)) {
            value = (T)obj;
            return true;
        }

        value = default!;
        return false;
    }

    public void SetValue<T>(IBattleProperty<T> property, T value) where T : notnull =>
        Properties[property] = value;

    public BattleProperties Clone() {
        BattleProperties battleProperties = new();

        foreach (KeyValuePair<IBattleProperty, object> property in Properties)
            battleProperties.Properties[property.Key] = property.Value;

        return battleProperties;
    }
}

public interface IBattleProperty {
    string Name { get; }
}

public interface IBattleProperty<out T> : IBattleProperty where T : notnull {
    T DefaultValue { get; }
}

public static class BattleProperty {
    static Dictionary<string, IBattleProperty> Properties { get; } = [];

    public static BattleProperty<IEntity> MapEntity { get; } = new("MapEntity");
    public static BattleProperty<MapInfo> MapInfo { get; } = new("MapInfo");

    public static BattleProperty<BattleType> Type { get; } = new("Type");
    public static BattleProperty<TimeSpan> WarmUpDuration { get; } = new("WarmUpDuration", TimeSpan.Zero);
    public static BattleProperty<bool> DamageEnabled { get; } = new("DamageEnabled", true);

    public static BattleProperty<bool> FriendlyFire { get; } = new("FriendlyFire", false);
    public static BattleProperty<bool> KillZoneEnabled { get; } = new("KillZoneEnabled", true);
    public static BattleProperty<bool> DisabledModules { get; } = new("DisabledModules", false);
    public static BattleProperty<int> MaxPlayers { get; } = new("MaxPlayers", 10);
    public static BattleProperty<int> TimeLimit { get; } = new("TimeLimit", 10);
    public static BattleProperty<GravityType> Gravity { get; } = new("Gravity");
    public static BattleProperty<BattleMode> BattleMode { get; } = new("BattleMode");

    public static IReadOnlyCollection<IBattleProperty> Entries => Properties.Values;

    public static void Register(IBattleProperty property) => Properties.Add(property.Name, property);

    public static IBattleProperty Get(string name) =>
        Properties.TryGetValue(name, out IBattleProperty? property)
            ? property
            : throw new KeyNotFoundException($"Property '{name}' not found");

    public static IBattleProperty? GetOrDefault(string name) => Properties.GetValueOrDefault(name);
}

public readonly struct BattleProperty<T> : IBattleProperty<T>, IEquatable<BattleProperty<T>> where T : notnull {
    public BattleProperty(string name, T defaultValue = default!) {
        Name = name;
        DefaultValue = defaultValue;

        BattleProperty.Register(this);
    }

    public string Name { get; }
    public T DefaultValue { get; }

    public bool Equals(BattleProperty<T> other) => Name == other.Name;

    public override bool Equals(object? obj) => obj is BattleProperty<T> other && Equals(other);

    public override int GetHashCode() => Name.GetHashCode();

    public static bool operator ==(BattleProperty<T> left, BattleProperty<T> right) => left.Equals(right);

    public static bool operator !=(BattleProperty<T> left, BattleProperty<T> right) => !left.Equals(right);

    public override string ToString() => $"BattleProperty<{typeof(T).Name}> ({Name}); Default: {DefaultValue})";
}

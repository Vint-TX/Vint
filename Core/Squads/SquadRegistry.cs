using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Vint.Core.Structures;

namespace Vint.Core.Squads;

public static class SquadRegistry {
    public static Registry Invites { get; } = new();
    public static Registry Requests { get; } = new();
}

public class Registry {
    static TimeSpan TimeToLive { get; } = TimeSpan.FromSeconds(10);
    BlockingMultiMap<long, Target> Storage { get; } = [];

    public bool Add(long source, long target) {
        RemoveExpired(source);
        return Storage.Add(source, new Target(target, DateTimeOffset.UtcNow + TimeToLive));
    }

    public bool Remove(long source, long targetId) =>
        RemoveExpiredOr(source, target => target.Id == targetId) > 0;

    public bool RemoveAll(long source, [NotNullWhen(true)] out FrozenSet<long>? targets) {
        if (Storage.Remove(source, out HashSet<Target>? values)) {
            targets = values.Select(target => target.Id).ToFrozenSet();
            return true;
        }

        targets = null;
        return false;
    }

    public bool Exists(long source, long targetId) {
        RemoveExpired(source);
        return Storage.TryGetValue(source, out HashSet<Target>? targets) &&
               targets.Any(target => target.Id == targetId);
    }

    int RemoveExpired(long source) =>
        Storage.RemoveWhere(source, target => target.ExpirationTime <= DateTimeOffset.UtcNow);

    int RemoveExpiredOr(long source, Predicate<Target> predicate) {
        int removedByPredicate = 0;

        Storage.RemoveWhere(source, target => {
            if (!predicate(target))
                return target.ExpirationTime <= DateTimeOffset.UtcNow;

            removedByPredicate++;
            return true;
        });

        return removedByPredicate;
    }

    readonly record struct Target(
        long Id,
        DateTimeOffset ExpirationTime
    ) {
        public override int GetHashCode() => Id.GetHashCode();
    }
}

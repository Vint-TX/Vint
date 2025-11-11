using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Vint.Core.Battle.Mode;
using Vint.Core.ECS.Components;
using Vint.Core.Exceptions;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Utils;

public static class Extensions {
    static ConcurrentDictionary<PropertyInfo, bool> NullabilityPool { get; } = new();

    extension(Type type) {
        public long GetProtocolId() =>
            type.GetCustomAttribute<ProtocolIdAttribute>()?.Id ??
            throw new ProtocolIdNotFoundException(type);

        public List<Type> DumpInterfaces() {
            if (!type.IsClass)
                throw new NotSupportedException("Type must be a class");

            HashSet<Type> allInterfaces = [..type.GetInterfaces()];
            Type? baseType = type.BaseType;

            if (baseType != null)
                allInterfaces.ExceptWith(baseType.GetInterfaces());

            return allInterfaces.ToList();
        }

        public bool IsList() => type.IsGenericType &&
                                type.GetGenericTypeDefinition()
                                    .IsAssignableFrom(typeof(List<>));

        public bool IsDictionary() => type.IsGenericType &&
                                      type.GetGenericTypeDefinition()
                                          .IsAssignableFrom(typeof(Dictionary<,>));

        public bool IsHashSet() => type.IsGenericType &&
                                   type.GetGenericTypeDefinition()
                                       .IsAssignableFrom(typeof(HashSet<>));
    }

    extension(PropertyInfo property) {
        public bool IsNullable() {
            if (NullabilityPool.TryGetValue(property, out bool isNullable))
                return isNullable;

            NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(property);
            isNullable = nullabilityInfo.ReadState == NullabilityState.Nullable || nullabilityInfo.WriteState == NullabilityState.Nullable;

            NullabilityPool.TryAdd(property, isNullable);
            return isNullable;
        }
    }

    extension(Stream input) {
        public void CopyTo(Stream output, int limit) {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(256);
            int read;

            while (limit > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, limit))) > 0) {
                output.Write(buffer, 0, read);
                limit -= read;
            }

            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    extension<TList>(TList list) where TList : IList {
        public TList Shuffle() {
            for (int i = list.Count - 1; i > 0; i--) {
                int n = Random.Shared.Next(i + 1);
                (list[i], list[n]) = (list[n], list[i]);
            }

            return list;
        }
    }

    extension<TElement>(IList<TElement> list) {
        public TElement RandomElement() =>
            list[Random.Shared.Next(list.Count)];
    }

    extension<T>(IEnumerable<T> enumerable) {
        public string ToString(bool extended) {
            List<T> list = enumerable.ToList();

            return list.Count == 0
                ? "Empty"
                : string.Join(", ", list.Select(obj => extended ? $"{obj}" : obj!.GetType().Name));
        }

        public T? SingleOrDefaultSafe(T? defaultValue = default) {
            try {
                return enumerable.SingleOrDefault(defaultValue);
            } catch {
                return defaultValue;
            }
        }

        [Pure]
        public bool HasDuplicates() {
            using IEnumerator<T> enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext())
                return false;

            HashSet<T> set = [];

            do {
                if (!set.Add(enumerator.Current))
                    return true;
            } while (enumerator.MoveNext());

            return false;
        }

        [Pure]
        public bool HasDuplicatesBy<TKey>(Func<T, TKey> selector) =>
            HasDuplicates(enumerable.Select(selector));

        [Pure]
        public bool ContainsAll(IEnumerable<T> second) =>
            !second.Except(enumerable).Any();

        [Pure]
        public bool ContainsAllBy<TKey>(IEnumerable<T> second, Func<T, TKey> selector) =>
            ContainsAll(enumerable.Select(selector), second.Select(selector));
    }

    extension<T>(T self) where T : IComponent {
        public T Clone() => (T)self.Clone();
    }

    extension(BattleMode mode) {
        public bool IsTeamMode() => mode is
            BattleMode.TDM or
            BattleMode.CTF or
            BattleMode.CP;
    }
}

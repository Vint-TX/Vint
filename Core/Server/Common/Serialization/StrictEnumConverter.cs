using System.Reflection;
using Newtonsoft.Json;

namespace Vint.Core.Server.Common.Serialization;

public class StrictEnumConverter<TEnum> : JsonConverter where TEnum : struct, Enum {
    static ulong AllValidValues { get; } = Enum.GetValues<TEnum>()
        .Select(value => Convert.ToUInt64(value))
        .Aggregate(0UL, (current, definedValue) => current | definedValue);

    public override bool CanConvert(Type objectType) => objectType == typeof(TEnum);

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        switch (reader.TokenType) {
            case JsonToken.Integer: {
                object value = Convert.ChangeType(reader.Value, objectType.GetEnumUnderlyingType())!;
                TEnum result = (TEnum)Enum.ToObject(objectType, value);

                if (!objectType.IsEnumDefined(value) && !IsFlagsEnumValid(result))
                    throw new JsonSerializationException($"Invalid value {reader.Value!} for enum {objectType.Name}");

                return result;
            }

            case JsonToken.String: {
                string? str = reader.Value?.ToString();

                if (!Enum.TryParse(str, out TEnum result))
                    throw new JsonSerializationException($"Invalid value '{str}' for enum {objectType.Name}");

                return result;
            }

            default:
                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing enum");
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        writer.WriteValue(Convert.ChangeType(value, typeof(TEnum).GetEnumUnderlyingType()));

    static bool IsFlagsEnumValid(TEnum value) {
        if (!typeof(TEnum).IsDefined(typeof(FlagsAttribute)))
            return false;

        ulong underlying = Convert.ToUInt64(value);
        // Value is 0, or only bits from valid values are set
        return underlying == 0 || (underlying & AllValidValues) == underlying;
    }
}

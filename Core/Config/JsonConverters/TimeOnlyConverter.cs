using Newtonsoft.Json;

namespace Vint.Core.Config.JsonConverters;

public class TimeOnlyConverter : JsonConverter<TimeOnly> {
    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer) =>
        writer.WriteValue(value.ToString());

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer) =>
        TimeOnly.TryParse((string?)reader.Value, out TimeOnly timeOnly) ? timeOnly : default;
}

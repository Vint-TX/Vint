using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Vint.Core.Server.API.Serialization;

class ResponseJsonSerializer {
    public ResponseJsonSerializer() {
        JsonSerializerSettings settings = new() {
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        Serializer = JsonSerializer.CreateDefault(settings);
    }

    JsonSerializer Serializer { get; }

    public async Task<string> Serialize(object? value) {
        string? json = TryGetJsonString(value);

        if (json != null)
            return json;

        StringBuilder sb = new(256);
        StringWriter sw = new(sb, CultureInfo.InvariantCulture);

        await using (JsonTextWriter jsonWriter = new(sw)) {
            jsonWriter.Formatting = Serializer.Formatting;
            Serializer.Serialize(jsonWriter, value, null);
        }

        return sw.ToString();
    }

    string? TryGetJsonString(object? value) {
        if (value is not string str || string.IsNullOrWhiteSpace(str))
            return null;

        try {
            JToken.Parse(str);
            return str;
        } catch (JsonReaderException) {
            return null;
        }
    }
}

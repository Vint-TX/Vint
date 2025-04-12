using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vint.Core.Server.API.Attributes;

namespace Vint.Core.Server.API.DTO.Base;

[MessageId(3)]
public record EnumerableClientDTO<T>(
    IEnumerable<T> Enumerable
) : IClientDTO {
    public JToken ToJson() => JArray.FromObject(Enumerable);

    public JToken ToJson(JsonSerializer serializer) => JArray.FromObject(Enumerable, serializer);
}

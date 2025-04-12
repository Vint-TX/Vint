using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vint.Core.Server.API.Attributes;

namespace Vint.Core.Server.API.DTO.Base;

[MessageId(2)]
public record PrimitiveClientDTO(
    object? Object
) : IClientDTO {
    public JToken ToJson() => Object == null ? JValue.CreateNull() : JObject.FromObject(Object);

    public JToken ToJson(JsonSerializer serializer) => Object == null ? JValue.CreateNull() : JObject.FromObject(Object, serializer);
}

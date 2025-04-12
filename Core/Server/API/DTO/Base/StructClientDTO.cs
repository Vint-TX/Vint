using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vint.Core.Server.API.DTO.Base;

public abstract record StructClientDTO : IClientDTO {
    public JToken ToJson() => JObject.FromObject(this);

    public JToken ToJson(JsonSerializer serializer) => JObject.FromObject(this, serializer);
}

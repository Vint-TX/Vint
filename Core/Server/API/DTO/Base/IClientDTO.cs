using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vint.Core.Server.API.DTO.Base;

public interface IClientDTO {
    JToken ToJson();

    JToken ToJson(JsonSerializer serializer);
}

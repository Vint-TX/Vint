using System.Reflection;
using EmbedIO;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Vint.Core.Server.API.Attributes.Deserialization;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromBodyAttribute : Attribute, IRequestDataAttribute<WebApiController> {
    static JsonSerializerSettings Settings { get; } = new() {
        MissingMemberHandling = MissingMemberHandling.Error,
        ContractResolver = new RequiredFieldsContractResolver(),
        DateParseHandling = DateParseHandling.DateTimeOffset
    };

    public async Task<object?> GetRequestDataAsync(WebApiController controller, Type type, string parameterName) {
        using TextReader reader = controller.HttpContext.OpenRequestText();
        string json = await reader.ReadToEndAsync();

        try {
            return JsonConvert.DeserializeObject(json, type, Settings);
        } catch (JsonSerializationException e) {
            throw HttpException.BadRequest(e.Message);
        }
    }
}

public class RequiredFieldsContractResolver : DefaultContractResolver {
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        property.Required = Required.AllowNull;
        return property;
    }
}

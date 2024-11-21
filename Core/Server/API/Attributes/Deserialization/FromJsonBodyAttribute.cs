using EmbedIO;
using EmbedIO.WebApi;
using Newtonsoft.Json;

namespace Vint.Core.Server.API.Attributes.Deserialization;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromJsonBodyAttribute : Attribute, IRequestDataAttribute<WebApiController> {
    static JsonSerializerSettings Settings { get; } = new() { MissingMemberHandling = MissingMemberHandling.Error };

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

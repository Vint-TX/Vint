using Vint.Core.Server.API.Attributes;

namespace Vint.Core.Server.API.Data.Status;

[MessageId(1)]
public record ErrorDTO(
    int Code,
    string Message,
    object? Data
) : IClientDTO {
    public static ErrorDTO BadRequest(string message, object? data = null) => new(400, message, data);
    public static ErrorDTO NotFound(string message, object? data = null) => new(404, message, data);
    public static ErrorDTO InternalServerError(string message, object? data = null) => new(500, message, data);
}

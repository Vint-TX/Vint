using Vint.Core.Server.API.Attributes;

namespace Vint.Core.Server.API.Data.Status;

[MessageId(2)]
public record SuccessDTO(
    int Code,
    string? Message,
    object? Data
) : IClientDTO {
    public static SuccessDTO Ok(object data, string? message = null) => new(200, message, data);
    public static SuccessDTO Created(object data, string? message = null) => new(201, message, data);
    public static SuccessDTO NoContent() => new(204, null, null);
}

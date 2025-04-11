using Vint.Core.Server.API.Attributes;

namespace Vint.Core.Server.API.Data.Error;

[MessageId(1)]
public record ErrorData(
    int Code,
    string Message,
    object? Data
) : IClientData;

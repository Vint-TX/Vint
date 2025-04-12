using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.DTO.Base;

namespace Vint.Core.Server.API.DTO.Error;

[MessageId(1)]
public record ErrorDTO(
    int Code,
    string Message,
    object? Data
) : StructClientDTO;

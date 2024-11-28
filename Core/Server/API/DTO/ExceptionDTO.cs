namespace Vint.Core.Server.API.DTO;

public record ExceptionDTO(
    string Message,
    object? Data
);

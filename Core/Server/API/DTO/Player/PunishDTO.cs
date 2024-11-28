namespace Vint.Core.Server.API.DTO.Player;

public record PunishDTO(
    string? Reason,
    TimeSpan? Duration
);

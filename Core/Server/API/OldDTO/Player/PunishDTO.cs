namespace Vint.Core.Server.API.OldDTO.Player;

public record PunishDTO(
    string? Reason,
    TimeSpan? Duration
);

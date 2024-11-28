namespace Vint.Core.Server.API.DTO.Invite;

public record InviteDTO(
    string Code,
    ushort Uses
);

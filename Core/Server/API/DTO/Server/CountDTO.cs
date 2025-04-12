using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.DTO.Base;

namespace Vint.Core.Server.API.DTO.Server;

[MessageId(7)]
public record CountDTO(
    int Connections,
    int Players,
    int MatchmakingBattles,
    int ArcadeBattles,
    int CustomBattles
) : StructClientDTO;

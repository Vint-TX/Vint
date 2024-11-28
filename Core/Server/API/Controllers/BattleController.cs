using EmbedIO;
using EmbedIO.WebApi;
using Vint.Core.Battles;
using Vint.Core.Server.API.Attributes.Methods;
using Vint.Core.Server.API.DTO.Battle;

namespace Vint.Core.Server.API.Controllers;

public class BattleController(
    IBattleProcessor battleProcessor
) : WebApiController {
    [Get("/")]
    public IEnumerable<BattleSummaryDTO> GetBattles() =>
        battleProcessor.Battles.Select(BattleSummaryDTO.FromBattle);

    [Get("/{id}")]
    public BattleDetailDTO GetBattle(long id) {
        Battle? battle = battleProcessor.FindByBattleId(id);

        if (battle == null)
            throw HttpException.NotFound($"Battle with id {id} does not exist");

        return BattleDetailDTO.FromBattle(battle);
    }
}

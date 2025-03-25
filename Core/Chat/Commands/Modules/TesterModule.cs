using Vint.Core.Battle.Tank;
using Vint.Core.Chat.Commands.Attributes;
using Vint.Core.Database.Models;

namespace Vint.Core.Chat.Commands.Modules;

[ChatCommandGroup("tester", "Commands for testers", PlayerGroups.None)] // todo PlayerGroups.None yet
public class TesterModule : ChatCommandModule {
    [RequireConditions(ChatCommandConditions.InRound), ChatCommand("spawnpoint", "Get last spawn point coordinates")]
    public async Task SpawnPoint(ChatCommandContext ctx) {
        BattleTank battleTank = ctx.Connection.LobbyPlayer!.Tanker!.Tank;

        await ctx.SendPrivateResponse($"Previous: {battleTank.PreviousSpawnPoint}");
        await ctx.SendPrivateResponse($"Current: {battleTank.SpawnPoint}");
    }
}

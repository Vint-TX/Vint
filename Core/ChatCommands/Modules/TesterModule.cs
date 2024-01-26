using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Database.Models;

namespace Vint.Core.ChatCommands.Modules;

[ChatCommandGroup("tester", "Commands for testers", PlayerGroups.None)] // todo PlayerGroups.None yet
public class TesterModule : ChatCommandModule {
    [RequireConditions(ChatCommandConditions.InBattle)]
    [ChatCommand("spawnpoint", "Get last spawn point coordinates")]
    public void SpawnPoint(ChatCommandContext ctx) =>
        ctx.SendPrivateResponse(ctx.Connection.BattlePlayer!.Tank!.SpawnPoint.ToString());
}
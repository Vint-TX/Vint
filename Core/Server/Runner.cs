using Vint.Core.Server.API;
using Vint.Core.Server.Game;
using Vint.Core.Server.Static;
using Vint.Core.Utils;

namespace Vint.Core.Server;

public class Runner(
    ApiServer apiServer,
    StaticServer staticServer,
    GameServer gameServer
) {
    public async Task Run() {
        await Extensions.WhenAllFastFail(
            staticServer.Start(),
            apiServer.Start(),
            gameServer.Start()
        );
    }
}

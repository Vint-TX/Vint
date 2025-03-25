using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Items.Module;

[ProtocolId(636407242256473252)]
public class UpgradeModuleWithXCrystalsEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        await connection.UpgradeModule(entities.Single(), true);
}

using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Items.Module;

[ProtocolId(636329559762986136)]
public class UpgradeModuleWithCrystalsEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) =>
        await connection.UpgradeModule(entities.Single(), false);
}

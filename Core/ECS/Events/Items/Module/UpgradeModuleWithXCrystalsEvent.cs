using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Items.Module;

[ProtocolId(636407242256473252)]
public class UpgradeModuleWithXCrystalsEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) =>
        await connection.UpgradeModule(entities.Single(), true);
}

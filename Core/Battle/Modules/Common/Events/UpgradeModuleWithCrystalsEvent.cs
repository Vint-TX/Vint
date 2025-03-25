using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Events;

[ProtocolId(636329559762986136)]
public class UpgradeModuleWithCrystalsEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        await connection.UpgradeModule(entities.Single(), false);
}

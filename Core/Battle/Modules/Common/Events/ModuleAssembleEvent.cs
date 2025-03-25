using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Items.Module;

[ProtocolId(1485504324992)]
public class ModuleAssembleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) =>
        await connection.AssembleModule(entities.First());
}

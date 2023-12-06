using Vint.Core.ECS.Components;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.Protocol.Commands;

public class EntityShareCommand(
    long entityId,
    TemplateAccessor? templateAccessor,
    params IComponent[] components
) : ICommand {
    [ProtocolPosition(0)] public long EntityId => entityId;
    [ProtocolPosition(1)] public TemplateAccessor? TemplateAccessor => templateAccessor;
    [ProtocolPosition(2)] [ProtocolCollection(varied: true)]
    public IComponent[] Components => components;

    public void Execute(PlayerConnection connection) => throw new NotImplementedException();
}
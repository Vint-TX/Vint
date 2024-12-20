using Vint.Core.ECS.Components;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Server.Game.Protocol.Commands;

public class EntityShareCommand(
    long entityId,
    TemplateAccessor? templateAccessor,
    params IComponent[] components
) : ICommand {
    [ProtocolPosition(0)] public long EntityId => entityId;
    [ProtocolPosition(1)] public TemplateAccessor? TemplateAccessor => templateAccessor;
    [ProtocolPosition(2), ProtocolCollection(varied: true)]
    public IComponent[] Components => components;

    public override string ToString() => $"EntityShare command {{ " +
                                         $"EntityId: {EntityId}, " +
                                         $"TemplateAccessor: {TemplateAccessor}, " +
                                         $"Components: {{ {Components.ToString(false)} }} }}";
}

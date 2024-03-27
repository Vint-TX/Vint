using System.Numerics;
using Vint.Core.ECS.Components.Battle.Flag;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Flag;

[ProtocolId(1431942342249)]
public class PedestalTemplate : EntityTemplate {
    public IEntity Create(Vector3 position, IEntity team, IEntity battle) => Entity("battle/modes/ctf",
        builder => builder
            .AddComponent(new FlagPedestalComponent(position))
            .AddComponentFrom<TeamGroupComponent>(team)
            .AddComponentFrom<BattleGroupComponent>(battle));
}
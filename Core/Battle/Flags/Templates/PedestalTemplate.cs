using System.Numerics;
using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Flags.Components;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Flags.Templates;

[ProtocolId(1431942342249)]
public class PedestalTemplate : EntityTemplate {
    public IEntity Create(Vector3 position, IEntity team, IEntity round) => Entity("battle/modes/ctf",
        builder => builder
            .AddComponent(new FlagPedestalComponent(position))
            .AddGroupComponent<TeamGroupComponent>(team)
            .AddGroupComponent<BattleGroupComponent>(round));
}

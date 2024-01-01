using Vint.Core.ECS.Components;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Leagues;

[ProtocolId(1503654626834)]
public class CurrentSeasonRewardForClientComponent(
    int leagueIndex
) : IComponent; //todo
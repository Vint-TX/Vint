using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Score.Events;

[ProtocolId(1439453338183)]
public class RoundUserStatisticsUpdatedEvent : IEvent;

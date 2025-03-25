using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Common.Events;

[ProtocolId(12346)]
public class BattleTimerUpdatedEvent : IEvent;

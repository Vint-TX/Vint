using System.Numerics;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Bonus;

[ProtocolId(-8402520789413485183)]
public class GoldBonusTemplate : BonusTemplate {
    public IEntity Create(Vector3 position, IEntity regionEntity, IEntity roundEntity) =>
        Create("battle/bonus/gold/cry", position, regionEntity, roundEntity);
}

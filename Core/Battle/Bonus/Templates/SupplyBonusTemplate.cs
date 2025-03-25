using System.Numerics;
using Vint.Core.Battle.Bonus;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Bonus;

[ProtocolId(5411677468097447480)]
public class SupplyBonusTemplate : BonusTemplate {
    public IEntity Create(BonusType type, Vector3 position, IEntity regionEntity, IEntity roundEntity) =>
        Create($"battle/bonus/{type.ToString().ToLower()}", position, regionEntity, roundEntity);
}

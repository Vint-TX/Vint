using System.Numerics;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Battle.Weapon;

[ProtocolId(1447917521601)]
public class DetachWeaponEvent : IServerEvent {
    public Vector3 AngularVelocity { get; private set; }
    public Vector3 Velocity { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity tankEntity = entities.Single();
        Tanker? tanker = connection.LobbyPlayer?.Tanker;
        BattleTank? tank = tanker?.Tank;

        if (tanker == null ||
            tank == null ||
            tank.Entities.Tank != tankEntity ||
            tank.StateManager.CurrentState is not Dead)
            return;

        await tanker.Round.Players.Send(this, tankEntity);
    }
}

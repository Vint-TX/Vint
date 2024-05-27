using Vint.Core.Battles.Type;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1547630520757)]
public class OpenCustomLobbyEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby ||
            connection.BattlePlayer!.Battle.TypeHandler is not CustomHandler customHandler) return;

        Player player = connection.Player;
        int price = player.IsPremium ? 0 : 1000;

        if (player.Crystals < price) return;

        await connection.ChangeCrystals(-price);

        if (customHandler.Owner != connection) return;

        await customHandler.OpenLobby();
    }
}

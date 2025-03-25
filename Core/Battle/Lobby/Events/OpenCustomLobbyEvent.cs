using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.Events;

[ProtocolId(1547630520757)]
public class OpenCustomLobbyEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby ||
            connection.LobbyPlayer.Lobby is not CustomLobby lobby ||
            lobby.Owner != connection) return;

        Database.Models.Player player = connection.Player;
        int price = player.IsPremium ? 0 : 1000;

        if (player.Crystals < price) return;

        await connection.ChangeCrystals(-price);
        await lobby.OpenLobby();
    }
}

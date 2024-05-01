using Vint.Core.Battles;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1547616531111)]
public class ConnectToCustomLobbyEvent : IServerEvent {
    public long LobbyId { get; private set; }

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (connection.InLobby) {
            connection.Send(new EnterBattleLobbyFailedEvent(true, false), connection.User);
            return Task.CompletedTask;
        }

        IBattleProcessor battleProcessor = connection.Server.BattleProcessor;
        Battles.Battle? battle = battleProcessor.FindByLobbyId(LobbyId);

        if (ValidateAndJoin(connection, battle)) return Task.CompletedTask;

        if (connection.Player.IsAdmin && LobbyId >= 0 && LobbyId < battleProcessor.BattlesCount) {
            battle = battleProcessor.FindByIndex((int)LobbyId);

            if (ValidateAndJoin(connection, battle)) return Task.CompletedTask;
        }

        connection.Send(new CustomLobbyNotExistsEvent(), connection.User);
        return Task.CompletedTask;
    }

    static bool ValidateAndJoin(IPlayerConnection connection, Battles.Battle? battle) {
        if (battle is not { CanAddPlayers: true } || !battle.LobbyEntity.HasComponent<OpenToConnectLobbyComponent>()) {
            connection.Send(new EnterBattleLobbyFailedEvent(false, true), connection.User);
            return false;
        }

        if (battle is not { TypeHandler: CustomHandler } && !connection.Player.IsAdmin) {
            connection.Send(new CustomLobbyNotExistsEvent(), connection.User);
            return false;
        }

        battle.AddPlayer(connection);
        return true;
    }
}

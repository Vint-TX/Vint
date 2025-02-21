using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.Server.Game;

namespace Vint.Core.ChatCommands.Attributes;

public class RequireConditionsAttribute(
    ChatCommandConditions conditions
) : BaseCheckAttribute {
    public ChatCommandConditions Conditions { get; } = conditions;

    public override string CheckFailedMessage => $"Conditions {Conditions} are not met";

    public override bool Check(ChatCommandContext context) {
        IPlayerConnection connection = context.Connection;
        ChatCommandConditions[] checkValues = Enum.GetValues<ChatCommandConditions>();
        bool returnValue = true;

        foreach (ChatCommandConditions condition in checkValues) {
            if (returnValue == false) break;

            if (condition == ChatCommandConditions.None) continue;

            if ((Conditions & condition) == condition) {
                returnValue &= condition switch {
                    ChatCommandConditions.InGarage => connection is { InLobby: false, Spectating: false },

                    ChatCommandConditions.InLobby => connection.InLobby &&
                                                     !connection.LobbyPlayer.InRound,

                    ChatCommandConditions.InRound => connection.InLobby &&
                                                     connection.LobbyPlayer.InRound,

                    ChatCommandConditions.AllInLobby => connection.InLobby &&
                                                        connection.LobbyPlayer.Lobby.Players.All(player => !player.InRound),

                    ChatCommandConditions.AllInRound => connection.InLobby &&
                                                        connection.LobbyPlayer.Lobby.Players.All(player => player.InRound),

                    ChatCommandConditions.LobbyOwner => connection.InLobby &&
                                                        connection.LobbyPlayer.Lobby is CustomLobby custom &&
                                                        custom.Owner == connection,

                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        return returnValue;
    }
}

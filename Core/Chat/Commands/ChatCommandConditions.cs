namespace Vint.Core.Chat.Commands;

[Flags]
public enum ChatCommandConditions {
    None = 0,
    InGarage = 1,
    LobbyOwner = 2,

    InRound = 4,
    AllInRound = 8,

    InLobby = 16,
    AllInLobby = 32
}

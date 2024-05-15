namespace Vint.Core.ChatCommands;

[Flags]
public enum ChatCommandConditions {
    None = 0,
    InGarage = 1,
    BattleOwner = 2,

    InBattle = 4,
    AllInBattle = 8,

    InLobby = 16,
    AllInLobby = 32
}

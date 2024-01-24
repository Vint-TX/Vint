namespace Vint.Core.ChatCommands;

[Flags]
public enum ChatCommandConditions {
    None = 0,
    BattleOwner = 1,

    InBattle = 2,
    AllInBattle = 4,

    InLobby = 8,
    AllInLobby = 16
}
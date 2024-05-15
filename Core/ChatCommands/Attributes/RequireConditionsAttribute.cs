using Vint.Core.Battles.Type;
using Vint.Core.Server;

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
                    ChatCommandConditions.InGarage => !connection.InLobby,

                    ChatCommandConditions.InLobby => connection.InLobby,

                    ChatCommandConditions.InBattle => connection.InLobby && connection.BattlePlayer!.InBattle,

                    ChatCommandConditions.AllInLobby => connection.InLobby &&
                                                        connection.BattlePlayer!.Battle.Players.All(battlePlayer => !battlePlayer.InBattleAsTank),

                    ChatCommandConditions.AllInBattle => connection.InLobby &&
                                                         connection.BattlePlayer!.Battle.Players.All(battlePlayer => battlePlayer.InBattle),

                    ChatCommandConditions.BattleOwner => connection.InLobby &&
                                                         connection.BattlePlayer!.Battle.TypeHandler is CustomHandler customHandler &&
                                                         customHandler.Owner == connection,

                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        return returnValue;
    }
}

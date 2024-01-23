using Vint.Core.Battles.Type;
using Vint.Core.Server;

namespace Vint.Core.ChatCommands.Attributes;

public class RequireConditionAttribute(
    ChatCommandCondition conditions
) : BaseCheckAttribute {
    public ChatCommandCondition Conditions { get; } = conditions;

    public override string CheckFailedMessage => $"Conditions {Conditions} are not met";

    public override bool Check(ChatCommandContext context) {
        IPlayerConnection connection = context.Connection;
        ChatCommandCondition[] checkValues = Enum.GetValues<ChatCommandCondition>();
        bool returnValue = true;
        
        foreach (ChatCommandCondition condition in checkValues) {
            if (returnValue == false) break;
            
            if ((Conditions & condition) == condition) {
                returnValue &= condition switch {
                    ChatCommandCondition.InLobby => connection.InLobby,
                    
                    ChatCommandCondition.InBattle => connection.InLobby && connection.BattlePlayer!.InBattle,
                    
                    ChatCommandCondition.AllInLobby => connection.InLobby &&
                                                       connection.BattlePlayer!.Battle.Players.All(battlePlayer => !battlePlayer.InBattleAsTank),
                    
                    ChatCommandCondition.AllInBattle => connection.InLobby &&
                                                        connection.BattlePlayer!.Battle.Players.All(battlePlayer => battlePlayer.InBattle),
                    
                    ChatCommandCondition.BattleOwner => connection.InLobby &&
                                                        connection.BattlePlayer!.Battle.TypeHandler is CustomHandler customHandler &&
                                                        customHandler.Owner == connection,
                        
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        return returnValue;
    }
}
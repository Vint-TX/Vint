using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Battle.Flag;

[ProtocolId(-1910863908782544246)]
public class FlagDropRequestEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattleAsTank) return;

        BattlePlayer battlePlayer = connection.BattlePlayer;
        Battles.Battle battle = battlePlayer.Battle;

        if (battle.ModeHandler is not CTFHandler ctf) return;

        Battles.Flag? flag = ctf.Flags.Values.SingleOrDefault(flag => flag.Entity == entities.ElementAt(0));

        if (flag?.StateManager.CurrentState is not Captured ||
            flag.TeamColor == battlePlayer.TeamColor) return;

        flag.Drop(true);
    }
}
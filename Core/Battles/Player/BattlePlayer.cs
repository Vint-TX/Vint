using Vint.Core.Battles.States;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Battle.User;
using Vint.Core.Server;

namespace Vint.Core.Battles.Player;

public class BattlePlayer {
    public BattlePlayer(IPlayerConnection playerConnection, Battle battle, IEntity? team, bool isSpectator) {
        PlayerConnection = playerConnection;
        Team = team;
        Battle = battle;
        IsSpectator = isSpectator;

        BattleUserTemplate battleUserTemplate = new();

        User = IsSpectator
                   ? battleUserTemplate.CreateAsSpectator(PlayerConnection.User, Battle.BattleEntity)
                   : battleUserTemplate.CreateAsTank(PlayerConnection.User, Battle.BattleEntity, Team);
    }

    public IPlayerConnection PlayerConnection { get; }
    public IEntity? Team { get; }
    public IEntity User { get; }

    public Battle Battle { get; }
    public BattleTank? Tank { get; private set; }

    public bool IsSpectator { get; }
    public bool InBattleAsTank => Tank != null;
    public bool InBattle { get; private set; }
    public bool Paused { get; set; }

    public DateTimeOffset BattleJoinTime { get; } = DateTimeOffset.UtcNow.AddSeconds(10);
    public DateTimeOffset? KickTime { get; set; }

    public void Init() {
        PlayerConnection.Share(Battle.BattleEntity, Battle.RoundEntity, Battle.BattleChatEntity);

        // todo modules & supplies

        PlayerConnection.User.AddComponent(Battle.BattleEntity.GetComponent<BattleGroupComponent>());
        Battle.ModeHandler.PlayerEntered(this);

        if (IsSpectator) {
            PlayerConnection.Share(User);
            InBattle = true;
        } else {
            Tank = new BattleTank(this);

            // todo modules

            InBattle = true;

            foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
                player.PlayerConnection.Share(Tank.Entities); // Share this player entities to other players in battle

            foreach (BattlePlayer spectator in Battle.Players.Where(player => player.IsSpectator))
                spectator.PlayerConnection.Share(PlayerConnection.User); // Share this player to spectators
        }

        PlayerConnection.Share(Battle.Players
            .Where(player => player != this && player.InBattleAsTank)
            .SelectMany(player => player.Tank!.Entities));
    }

    public void Tick() {
        if (Tank == null) return;
        
        Tank.StateManager.Tick();

        if (Tank.CollisionsPhase != Battle.BattleEntity.GetComponent<BattleTankCollisionsComponent>().SemiActiveCollisionsPhase) return;

        if (Tank.Tank.HasComponent<TankStateTimeOutComponent>())
            Tank.Tank.RemoveComponent<TankStateTimeOutComponent>();
            
        Battle.BattleEntity.ChangeComponent<BattleTankCollisionsComponent>(component => 
            component.SemiActiveCollisionsPhase++);
            
        Tank.StateManager.SetState(new Active(Tank.StateManager));
        Tank.SetHealth(Tank.MaxHealth);
    }
}
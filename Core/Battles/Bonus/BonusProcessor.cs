using Vint.Core.Battles.Bonus.Type;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;
using BonusInfo = Vint.Core.Config.MapInformation.Bonus;

namespace Vint.Core.Battles.Bonus;

public interface IBonusProcessor {
    public void Start();
    
    public void Tick();
    
    public void Take(BonusBox bonus, BattleTank tank);
    
    public void ShareEntities(IPlayerConnection connection);
    
    public void UnshareEntities(IPlayerConnection connection);
    
    public BonusBox? FindByEntity(IEntity bonusEntity);
    
    public bool DropBonus(BonusType type);
}

public class BonusProcessor : IBonusProcessor {
    public BonusProcessor(Battle battle, IDictionary<BonusType, IEnumerable<BonusInfo>> bonusesInfos) {
        List<BonusBox> bonuses = [];
        
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach ((BonusType type, IEnumerable<BonusInfo> bonusesInfo) in bonusesInfos) {
            if (type == BonusType.Gold && battle.TypeHandler is not MatchmakingHandler) continue;
            
            IEnumerable<BonusBox> bonusBoxes = type switch {
                BonusType.Repair => bonusesInfo.Select(bonusInfo => new RepairBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                BonusType.Armor => bonusesInfo.Select(bonusInfo => new ArmorBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                BonusType.Damage => bonusesInfo.Select(bonusInfo => new DamageBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                BonusType.Speed => bonusesInfo.Select(bonusInfo => new SpeedBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                BonusType.Gold => bonusesInfo.Select(bonusInfo => new GoldBox(battle, bonusInfo.Position, bonusInfo.HasParachute)),
                _ => throw new ArgumentException("Unexpected bonus type", nameof(bonusesInfos))
            };
            
            bonuses.AddRange(bonusBoxes);
        }
        
        Bonuses = bonuses.Shuffle();
    }
    
    IReadOnlyList<BonusBox> Bonuses { get; }
    
    public void Start() {
        foreach (SupplyBox supply in Bonuses.OfType<SupplyBox>()) {
            supply.ShareRegion();
            supply.StateManager.SetState(
                new Cooldown(supply.StateManager, TimeSpan.FromSeconds(Random.Shared.Next(60))));
        }
    }
    
    public void Tick() {
        foreach (BonusBox bonus in Bonuses)
            bonus.Tick();
    }
    
    public void Take(BonusBox bonus, BattleTank tank) {
        bonus.Take(tank);
        
        // todo smth
    }
    
    public void ShareEntities(IPlayerConnection connection) {
        List<IEntity> entities = new(Bonuses.Count * 2);
        
        foreach (BonusBox bonus in Bonuses) {
            if (bonus.RegionEntity == null || bonus is GoldBox && bonus.StateManager.CurrentState is None) continue;
            
            entities.Add(bonus.RegionEntity);
            
            if (bonus.Entity != null)
                entities.Add(bonus.Entity);
        }
        
        connection.ShareIfUnshared(entities);
    }
    
    public void UnshareEntities(IPlayerConnection connection) {
        List<IEntity> entities = new(Bonuses.Count * 2);
        
        foreach (BonusBox bonus in Bonuses) {
            if (bonus.RegionEntity != null)
                entities.Add(bonus.RegionEntity);
            
            if (bonus.Entity != null)
                entities.Add(bonus.Entity);
        }
        
        connection.UnshareIfShared(entities);
    }
    
    public BonusBox? FindByEntity(IEntity bonusEntity) =>
        Bonuses.FirstOrDefault(bonus => bonus.Entity == bonusEntity);
    
    public bool DropBonus(BonusType type) {
        BonusBox? bonus = Bonuses.FirstOrDefault(bonus => bonus.Type == type && bonus.StateManager.CurrentState is not Spawned);
        
        if (bonus == null) return false;
        
        bonus.Drop();
        return true;
    }
}
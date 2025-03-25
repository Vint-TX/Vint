using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Components.DailyBonus;
using Vint.Core.ECS.Components.Fraction;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.User;

[ProtocolId(1433752208915)]
public class UserTemplate : EntityTemplate {
    public IEntity Create(Player player) => Entity(null,
        builder => builder
            .AddComponent<UserComponent>()
            .AddComponent(new UserPublisherComponent { OwnerUserId = player.Id })
            .AddComponent(new RegistrationDateComponent(player.RegistrationTime) { OwnerUserId = player.Id })
            .AddComponent(new UserAvatarComponent(player.CurrentAvatarId))
            .AddComponent(new UserUidComponent(player.Username))
            .AddComponent(new UserCountryComponent(player.CountryCode) { OwnerUserId = player.Id })
            .AddComponent(new UserSubscribeComponent(player.Subscribed) { OwnerUserId = player.Id })
            .AddComponent(new ConfirmedUserEmailComponent(player.Email, player.Subscribed) { OwnerUserId = player.Id })
            .AddComponent(new PersonalChatOwnerComponent { OwnerUserId = player.Id })
            .AddComponent(new BlackListComponent { OwnerUserId = player.Id })
            .AddComponent(new UserExperienceComponent(player.Experience))
            .AddComponent(new UserRankComponent(player.Rank))
            .AddComponent(new UserMoneyComponent(player.Crystals) { OwnerUserId = player.Id })
            .AddComponent(new UserXCrystalsComponent(player.XCrystals) { OwnerUserId = player.Id })
            .AddComponent(new QuestReadyComponent { OwnerUserId = player.Id })
            .AddComponent(new DailyBonusReadyComponent { OwnerUserId = player.Id })
            .AddComponent(new UserReputationComponent(player.Reputation))
            // todo .AddComponent(new TutorialCompleteIdsComponent())
            .AddComponent(new FractionUserScoreComponent(player.FractionScore) { OwnerUserId = player.Id })
            .AddComponent(new UserStatisticsComponent(player.Id))
            .AddComponent(new FavoriteEquipmentStatisticsComponent(player.Id))
            .AddComponent(new KillsEquipmentStatisticsComponent(player.Id))
            .AddComponent(new BattleLeaveCounterComponent(player.DesertedBattlesCount, player.NeedGoodBattlesCount))
            .AddComponent(new GameplayChestScoreComponent(player.GameplayChestScore) { OwnerUserId = player.Id })
            .AddGroupComponent<LeagueGroupComponent>(player.LeagueEntity)
            .AddGroupComponent<UserGroupComponent>()
            .ThenExecuteIf(_ => player.IsAdmin, entity => entity.AddComponent<UserAdminComponent>())
            .ThenExecuteIf(_ => player.IsModerator, entity => entity.AddComponent<ModeratorComponent>())
            .ThenExecuteIf(_ => player.IsTester,
                entity => {
                    entity.AddComponent(new ClosedBetaQuestAchievementComponent { OwnerUserId = player.Id });
                    entity.AddComponent<UserTesterComponent>();
                })
            .WithId(player.Id));
}

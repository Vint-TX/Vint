using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Components.Fraction;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Templates.User;

[ProtocolId(1433752208915)]
public class UserTemplate : EntityTemplate {
    public IEntity Create(Player player) => Entity(null,
        builder => builder
            .AddComponent<UserComponent>()
            .AddComponent<UserOnlineComponent>()
            .AddComponent<UserPublisherComponent>()
            .AddComponent(new RegistrationDateComponent(player.RegistrationTime))
            .AddComponent(new UserUidComponent(player.Username))
            .AddComponent(new UserCountryComponent(player.CountryCode))
            .AddComponent(new UserSubscribeComponent(player.Subscribed))
            .AddComponent(new ConfirmedUserEmailComponent(player.Email, player.Subscribed))
            .AddComponent(new PersonalChatOwnerComponent())
            .AddComponent(new BlackListComponent())
            .AddComponent(new UserExperienceComponent(player.Experience))
            .AddComponent(new UserRankComponent(player.Rank))
            .AddComponent(new UserMoneyComponent(player.Crystals))
            .AddComponent(new UserXCrystalsComponent(player.XCrystals))
            .AddComponent<QuestReadyComponent>()
            .AddComponent(new UserReputationComponent(player.Reputation))
            // todo .AddComponent(new TutorialCompleteIdsComponent())
            .AddComponent(new FractionUserScoreComponent(player.FractionScore))
            .AddComponent(new UserStatisticsComponent(player.Id))
            .AddComponent(new FavoriteEquipmentStatisticsComponent(player.Id))
            .AddComponent(new KillsEquipmentStatisticsComponent(player.Id))
            .AddComponent(new BattleLeaveCounterComponent(player.DesertedBattlesCount, player.NeedGoodBattlesCount))
            .AddComponent(new GameplayChestScoreComponent(player.GameplayChestScore))
            .AddGroupComponent<LeagueGroupComponent>(player.LeagueEntity)
            .AddGroupComponent<UserGroupComponent>()
            .ThenExecuteIf(_ => player.IsAdmin, entity => entity.AddComponent<UserAdminComponent>())
            .ThenExecuteIf(_ => player.IsModerator, entity => entity.AddComponent<ModeratorComponent>())
            .ThenExecuteIf(_ => player.IsTester,
                entity => {
                    entity.AddComponent<ClosedBetaQuestAchievementComponent>();
                    entity.AddComponent<UserTesterComponent>();
                })
            .WithId(player.Id));

    public IEntity CreateFake(IPlayerConnection connection, Player player) => Entity(null,
        builder => builder
            .AddComponent<UserComponent>()
            .AddComponent<UserPublisherComponent>()
            .AddComponent(new RegistrationDateComponent(player.RegistrationTime))
            .AddComponent(new UserUidComponent(player.Username))
            .AddComponent(new UserExperienceComponent(player.Experience))
            .AddComponent(new UserRankComponent(player.Rank))
            .AddComponent(new UserReputationComponent(player.Reputation))
            .AddComponent(new FractionUserScoreComponent(player.FractionScore))
            .AddComponent(new UserStatisticsComponent(player.Id))
            .AddComponent(new FavoriteEquipmentStatisticsComponent(player.Id))
            .AddComponent(new KillsEquipmentStatisticsComponent(player.Id))
            .AddComponent(new UserAvatarComponent(connection, player.CurrentAvatarId))
            .ThenExecuteIf(_ => player.IsAdmin, entity => entity.AddComponent<UserAdminComponent>())
            .ThenExecuteIf(_ => player.IsModerator, entity => entity.AddComponent<ModeratorComponent>())
            .ThenExecuteIf(_ => player.IsTester, entity => entity.AddComponent<UserTesterComponent>())
            .AddGroupComponent<LeagueGroupComponent>(player.LeagueEntity)
            .AddGroupComponent<UserGroupComponent>()
            .WithId(player.Id),
        true);
}

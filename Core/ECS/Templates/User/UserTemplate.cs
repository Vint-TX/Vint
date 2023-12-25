using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Chat;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.User;

[ProtocolId(1433752208915)]
public class UserTemplate : EntityTemplate {
    public IEntity Create(Player player) {
        IEntity user = Entity(null,
            builder => {
                builder
                    .AddComponent(new UserComponent())
                    .AddComponent(new UserOnlineComponent())
                    .AddComponent(new UserPublisherComponent())
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
                    .AddComponent(new QuestReadyComponent());

                if (player.IsAdmin)
                    builder.AddComponent(new UserAdminComponent());

                if (player.IsTester)
                    builder.AddComponent(new ClosedBetaQuestAchievementComponent());
            });

        user.AddComponent(new UserGroupComponent(user));

        return user;
    }
}
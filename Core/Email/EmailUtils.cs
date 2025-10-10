using System.Net.Mail;
using JetBrains.Annotations;
using LinqToDB;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Email.Components;
using Vint.Core.Notification.Templates;
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Email;

public static class EmailUtils {
    static List<EmailReward> Rewards { get; } = ConfigManager.GetComponent<EmailRewardsComponent>("email_rewards").Rewards;

    [Pure]
    public static async Task<EmailValidationResult> Validate(string email, bool includeUnconfirmed) {
        try { // will throw if email is invalid
            _ = new MailAddress(email);
        } catch {
            return EmailValidationResult.Invalid;
        }

        await using DbConnection db = new();

        if (await db.Players.AnyAsync(player => player.Email == email && (includeUnconfirmed || player.EmailConfirmed)))
            return EmailValidationResult.Occupied;

        return EmailValidationResult.Vacant;
    }

    public static async Task ReceiveEmailRewards(IPlayerConnection connection) {
        foreach (EmailReward reward in Rewards) {
            IEntity rewardEntity = GlobalEntities.AllMarketTemplateEntities.First(entity => entity.Id == reward.Id);

            if (!await connection.CanOwnItem(rewardEntity)) continue;

            await connection.PurchaseItem(rewardEntity, reward.Amount, 0, false, false);
            await connection.Share(new NewItemNotificationTemplate().CreateRegular(connection.UserContainer.Entity, rewardEntity, reward.Amount));
        }
    }
}

public enum EmailValidationResult : byte {
    Vacant,
    Occupied,
    Invalid,
}

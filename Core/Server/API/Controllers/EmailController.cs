using LinqToDB;
using LinqToDB.Async;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Email;
using Vint.Core.Email.Components;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Status;
using Vint.Core.Server.API.Utils;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;

namespace Vint.Core.Server.API.Controllers;

public class EmailController(
    GameServer server,
    IServiceScopeFactory serviceScopeFactory
) : IApiController {
    [MessageId(46)]
    public async Task<IClientDTO> ConfirmEmail(long playerId, string token) {
        await using DbConnection db = new();
        EmailConfirmation? emailConfirmation = await db.EmailConfirmations.FirstOrDefaultAsync(c => c.PlayerId == playerId && c.Token == token);

        if (emailConfirmation == null)
            return ErrorDTO.NotFound("Email confirmation not found");

        if (DateTimeOffset.UtcNow > emailConfirmation.ExpiresAt || emailConfirmation.Invalidated)
            return ErrorDTO.BadRequest("Email confirmation has expired");

        if (emailConfirmation.Used)
            return ErrorDTO.BadRequest("Email confirmation has already been used");

        await db.BeginTransactionAsync();
        await db.Players
            .Where(player => player.Id == playerId)
            .Set(player => player.Email, emailConfirmation.NewEmail)
            .Set(player => player.EmailConfirmed, true)
            .UpdateAsync();

        await db.Players
            .Where(player => player.Email == emailConfirmation.NewEmail && !player.EmailConfirmed && player.Id != playerId)
            .Set(player => player.Email, (string?)null)
            .Set(player => player.EmailConfirmed, false)
            .UpdateAsync();

        await db.EmailConfirmations
            .Where(c => c.Id == emailConfirmation.Id)
            .Set(c => c.Used, true)
            .Set(c => c.UsedAt, DateTimeOffset.UtcNow)
            .UpdateAsync();

        await db.CommitTransactionAsync();

        IPlayerConnection? connection = server.FindConnection(playerId);

        if (connection != null) {
            Player player = connection.Player;
            IEntity user = connection.UserContainer.Entity;

            player.Email = emailConfirmation.NewEmail;
            player.EmailConfirmed = true;

            await user.RemoveComponentIfPresent<ConfirmedUserEmailComponent>();
            await user.RemoveComponentIfPresent<UnconfirmedUserEmailComponent>();

            await user.AddComponent(new ConfirmedUserEmailComponent(emailConfirmation.NewEmail));

            if (!player.EmailRewardsReceived) {
                await EmailUtils.ReceiveEmailRewards(connection);

                await db.Players.Where(p => p.Id == player.Id)
                    .Set(p => p.EmailRewardsReceived, true)
                    .UpdateAsync();

                player.EmailRewardsReceived = true;
            }
        }

        await using AsyncServiceScope serviceScope = serviceScopeFactory.CreateAsyncScope();
        ApiServer apiServer = serviceScope.ServiceProvider.GetRequiredService<ApiServer>();

        if (!string.IsNullOrWhiteSpace(emailConfirmation.OldEmail) && emailConfirmation.OldEmail != emailConfirmation.NewEmail) {
            await apiServer.EmailChanged(emailConfirmation.PlayerId,
                emailConfirmation.OldEmail ?? "",
                emailConfirmation.NewEmail,
                DateTimeOffset.UtcNow);
        }

        return SuccessDTO.NoContent();
    }
}

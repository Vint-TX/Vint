using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Database;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Player;
using Vint.Core.Server.API.Data.Status;
using Vint.Core.Server.API.Utils;

namespace Vint.Core.Server.API.Controllers;

public class NewsController(
    IServiceScopeFactory serviceScopeFactory
) : IApiController {
    [MessageId(48)]
    public async Task<IClientDTO> GetSubscribers(string countryCode, int from, int count = 500) {
        from = Math.Max(0, from - 1);

        await using DbConnection db = new();
        List<PlayerSummaryData> subscribers = await db.Players
            .Where(player => player.NewsletterSubscribed && player.CountryCode == countryCode)
            .Skip(from)
            .Take(count)
            .Select(player => PlayerSummaryData.FromPlayer(player))
            .ToListAsync();

        return SuccessDTO.Ok(subscribers);
    }

    [MessageId(49)]
    public async Task<IClientDTO> NewsAvailable(string title, string subtitle, string imageUrl, string language, string url) {
        await using AsyncServiceScope serviceScope = serviceScopeFactory.CreateAsyncScope();
        ApiServer apiServer = serviceScope.ServiceProvider.GetRequiredService<ApiServer>();

        await apiServer.NewsAvailable(title, subtitle, imageUrl, language, url);
        return SuccessDTO.NoContent();
    }
}

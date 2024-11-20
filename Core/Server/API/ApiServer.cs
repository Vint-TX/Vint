using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using LinqToDB;
using Serilog;
using Swan;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Utils;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Server.API;

public class ApiServer {
    const ushort Port = 5051;

    public ApiServer() {
        Server = new WebServer(options => options
                .WithUrlPrefix($"http://localhost:{Port}/")
                .WithMode(HttpListenerMode.Microsoft))
            .HandleHttpException(HandleHttpException)
            .WithWebApi("/invites", async (context, obj) => {

            }, module => module.WithController<InvitesController>());

        Server.StateChanged += (_, e) => Logger.Debug("State changed: {Old} => {New}", e.OldState, e.NewState);
    }

    ILogger Logger { get; } = Log.Logger.ForType(typeof(ApiServer));
    WebServer Server { get; }

    public async Task Start() {
        await Server.RunAsync();
    }

    async Task HandleHttpException(IHttpContext context, IHttpException exception) {
        context.Response.StatusCode = exception.StatusCode;

        switch (exception.StatusCode) {
            case 404:
                // await context.SendDataAsync(exception.DataObject);
                await context.SendStandardHtmlAsync(404);
                break;
            default:
                await HttpExceptionHandler.Default(context, exception);
                break;
        }
    }
}

public class InvitesController : WebApiController {
    [Route(HttpVerbs.Get, "/")]
    public async Task<IEnumerable<Invite>> GetInvites() {
        await using DbConnection db = new();

        Invite[] invites = db.Invites.ToArray();
        return invites;
    }

    [Route(HttpVerbs.Get, "/{id}")]
    public async Task<string> GetInvite(long id) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Id == id);

        if (invite == null) {
            throw HttpException.NotFound($"Invite {id} not found");
        }

        return invite.ToJson();
    }

    [Route(HttpVerbs.Delete, "/{id}")]
    public async Task DeleteInvite(long id) {
        await using DbConnection db = new();
        Invite? invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Id == id);

        if (invite == null) {
            throw HttpException.NotFound($"Invite {id} not found");
        }

        await db.DeleteAsync(invite);
    }
}

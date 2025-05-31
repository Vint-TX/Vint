using EmbedIO;
using Serilog;
using Vint.Core.Logging;

namespace Vint.Core.Server.Common.Middlewares;

public class LoggingModule<TServer>(
    string baseRoute
) : WebModuleBase(baseRoute) {
    ILogger BaseLogger { get; } = Log.Logger.ForType<TServer>();

    public override bool IsFinalHandler => false;

    protected override Task OnRequestAsync(IHttpContext context) {
        IHttpRequest request = context.Request;

        if (request == null!)
            return Task.CompletedTask;

        BaseLogger.WithEndPoint(request).Information("{Method} {Url}", request.HttpMethod, request.RawUrl);
        return Task.CompletedTask;
    }
}

using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Web;
using NetCoreServer;
using Serilog;
using Vint.Core.Config;
using Vint.Core.Discord;
using Vint.Core.Utils;

namespace Vint.Core.Server;

public class StaticServer(
    IPAddress host,
    ushort port
) : HttpServer(host, port) {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(StaticServer));

    protected override StaticServerSession CreateSession() => new(this);

    protected override void OnStarted() => Logger.Information("Started");

    protected override void OnError(SocketError error) => Logger.Error("Static server caught an error: {Error}", error);
}

public class StaticServerSession(
    HttpServer server
) : HttpSession(server) {
    string Host { get; set; } = null!;
    string Root { get; } = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "StaticServer");
    ILogger Logger { get; set; } = Log.Logger.ForType(typeof(StaticServerSession));

    protected override void OnConnecting() =>
        Logger = Logger.WithEndPoint((IPEndPoint)Socket.RemoteEndPoint!);

    protected override void OnReceivedRequestHeader(HttpRequest request) {
        for (int i = 0; i < request.Headers; i++) {
            (string header, string value) = request.Header(i);

            switch (header) {
                case "X-Real-IP": // nginx proxying
                    Logger = Logger.WithEndPoint(IPEndPoint.Parse(value));
                    break;

                case "Host":
                    Host = value;
                    break;
            }
        }
    }

    protected override void OnReceivedRequest(HttpRequest request) {
        Logger.Information("{Method} {Url}", request.Method, request.Url);

        if (request.Method != "GET") {
            SendResponseAsync(Response.MakeErrorResponse(400));
            return;
        }

        string[] urlParts = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (urlParts.Length == 0) {
            SendResponseAsync(Response.MakeErrorResponse(400));
            return;
        }

        if (urlParts.Last().Contains('?'))
            urlParts[^1] = urlParts[^1][..urlParts[^1].IndexOf('?')];

        string requestedEntry = Path.Combine(Root, string.Join('/', urlParts));

        switch (urlParts[0]) {
            case "config": {
                if (requestedEntry.EndsWith("init.yml")) {
                    SendResponseAsync(File.Exists(requestedEntry)
                                          ? Response.MakeGetResponse(File.ReadAllText(requestedEntry)
                                              .Replace("*host*", Host)
                                              .Replace("*ip*", Host.Split(':')[0]))
                                          : Response.MakeErrorResponse(404));
                } else if (requestedEntry.EndsWith("config.tar.gz")) {
                    string locale = urlParts[^2];

                    locale = locale.ToLower() switch {
                        "ru" => "ru",
                        "en" => "en",
                        _ => "en"
                    };

                    SendResponseAsync(ConfigManager.TryGetConfig(locale, out byte[]? config)
                                          ? Response.MakeGetResponse(config)
                                          : Response.MakeErrorResponse(404));
                } else SendResponseAsync(Response.MakeErrorResponse(404));

                break;
            }

            case "state":
            case "update": {
                SendResponseAsync(File.Exists(requestedEntry)
                                      ? Response.MakeGetResponse(File.ReadAllText(requestedEntry)
                                          .Replace("*host*", Host))
                                      : Response.MakeErrorResponse(404));

                break;
            }

            case "resources": {
                SendResponseAsync(File.Exists(requestedEntry)
                                      ? Response.MakeGetResponse(File.ReadAllBytes(requestedEntry))
                                      : Response.MakeErrorResponse(404));

                break;
            }

            case "discord" when urlParts[1] == "auth": {
                NameValueCollection query = HttpUtility.ParseQueryString(request.Url.Split('?').Last());

                string? state = query["state"];
                string? code = query["code"];
                DiscordLinkRequest linkRequest = ConfigManager.DiscordLinkRequests.SingleOrDefault(req => req.State == state);

                if (state == null || code == null || linkRequest == default) {
                    SendResponseAsync(Response.MakeErrorResponse(400));
                    return;
                }

                ConfigManager.DiscordLinkRequests.TryRemove(linkRequest);

                /*SendResponseAsync(new HttpResponse().SetBegin(301).SetHeader("Location", "https://vint-official.site/"));
                Disconnect();*/

                bool? result = ConfigManager.NewLinkRequest?.Invoke(code, linkRequest.UserId).GetAwaiter().GetResult();

                SendResponseAsync(result == true
                    ? Response.MakeGetResponse("Your Discord account is successfully linked!")
                    : Response.MakeErrorResponse(400, "Account is not linked, contact the administrators for support"));
                break;
            }

            default:
                SendResponseAsync(Response.MakeErrorResponse(404));
                break;
        }
    }

    protected override void OnReceivedRequestError(HttpRequest request, string error) =>
        Logger.Error("{Method} {Url} failed with error: {Error}", request.Method, request.Url, error);

    protected override void OnError(SocketError error) =>
        Logger.Error("Session caught an error: {Error}", error);
}

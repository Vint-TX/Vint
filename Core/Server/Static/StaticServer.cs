using System.Collections.Specialized;
using System.Net;
using Serilog;
using Vint.Core.Config;
using Vint.Core.Discord;
using Vint.Core.Utils;

namespace Vint.Core.Server.Static;

public class StaticServer {
    const ushort Port = 8080;

    ILogger Logger { get; } = Log.Logger.ForType(typeof(StaticServer));
    HttpListener Listener { get; } = new();

    bool IsStarted { get; set; }
    bool IsAccepting { get; set; }

    string Resources { get; } = Path.Combine(Directory.GetCurrentDirectory(), "Resources");

    public async Task Start() {
        if (IsStarted) return;

        IsStarted = true;

        Listener.Prefixes.Add($"http://*:{Port}/");
        Listener.Start();

        OnStarted();
        await Accept();

        IsStarted = false;
    }

    async Task Accept() {
        if (IsAccepting) return;

        IsAccepting = true;

        while (Listener.IsListening) {
            try {
                HttpListenerContext context = await Listener.GetContextAsync();
                ILogger logger = Logger;

                string? ip = context.Request.Headers["X-Real-IP"];

                if (!string.IsNullOrWhiteSpace(ip) &&
                    IPEndPoint.TryParse(ip, out IPEndPoint? ipAddress))
                    logger = logger.WithEndPoint(ipAddress);

                await ProcessRequest(context, logger);
            } catch (Exception e) {
                Logger.Error(e, "");
            }
        }

        IsAccepting = false;
    }

    async Task ProcessRequest(HttpListenerContext context, ILogger logger) {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        Uri? url = request.Url;

        logger.Information("{Method} {Url}", request.HttpMethod, request.RawUrl);

        if (request.HttpMethod != "GET" ||
            url == null) {
            SendError(response, 400);
            return;
        }

        string[] urlParts = url.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (urlParts.Length == 0) {
            SendError(response, 400);
            return;
        }

        string requestedEntry = Path.Join(Resources, url.AbsolutePath);

        switch (urlParts[0]) {
            case "state": {
                await SendResponse(response, "state: 0");
                break;
            }

            case "config": {
                if (urlParts[1] == "init.yml")
                    await ProcessTextRequest(response, requestedEntry);
                else
                    await ProcessConfigRequest(response, urlParts);

                break;
            }

            case "discord" when urlParts[1] == "auth": {
                NameValueCollection query = request.QueryString;

                string? state = query["state"];
                string? code = query["code"];
                DiscordLinkRequest linkRequest = ConfigManager.DiscordLinkRequests.SingleOrDefault(req => req.State == state);

                if (state == null ||
                    code == null ||
                    linkRequest == default) {
                    SendError(response, 400);
                    return;
                }

                ConfigManager.DiscordLinkRequests.TryRemove(linkRequest);

                bool? result = ConfigManager
                    .NewLinkRequest
                    ?.Invoke(code, linkRequest.UserId)
                    .GetAwaiter()
                    .GetResult();

                if (result == true)
                    await SendResponse(response, "Your Discord account is successfully linked!");
                else
                    await SendResponse(response, "Account is not linked, contact the administrators for support");

                break;
            }

            default: {
                SendError(response, 404);
                break;
            }
        }
    }

    void OnStarted() =>
        Logger.Information("Started");

    static async Task ProcessConfigRequest(HttpListenerResponse response, string[] urlParts) {
        if (urlParts.Length < 2) {
            SendError(response, 400);
            return;
        }

        string locale = urlParts[^2]
            .ToLower() switch {
            "ru" => "ru",
            "en" => "en",
            _ => "en"
        };

        if (ConfigManager.TryGetConfig(locale, out byte[]? config))
            await SendResponse(response, config);
        else SendError(response, 404);
    }

    static async Task ProcessTextRequest(HttpListenerResponse response, string requestedEntry) {
        if (!File.Exists(requestedEntry)) {
            SendError(response, 404);
            return;
        }

        await SendResponse(response, await File.ReadAllTextAsync(requestedEntry));
    }

    static void SendError(HttpListenerResponse response, int statusCode) {
        response.StatusCode = statusCode;
        response.Close();
    }

    static async Task SendResponse(HttpListenerResponse response, string content) {
        await using (Stream output = response.OutputStream)
        await using (StreamWriter outputWriter = new(output))
            await outputWriter.WriteAsync(content);

        response.Close();
    }

    static async Task SendResponse(HttpListenerResponse response, byte[] content) {
        await using (Stream output = response.OutputStream)
            await output.WriteAsync(content);

        response.Close();
    }
}

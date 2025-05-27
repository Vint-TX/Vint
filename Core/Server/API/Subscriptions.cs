namespace Vint.Core.Server.API;

[Flags]
public enum Subscriptions : uint {
    None = 0,
    Reports = 1 << 0,
    Logs = 1 << 1,
    Players = 1 << 2,
    News = 1 << 3,
}

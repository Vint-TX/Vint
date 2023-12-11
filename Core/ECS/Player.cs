using Serilog;
using Vint.Utils;

namespace Vint.Core.ECS;

public class Player{
    public Player(ILogger connectionLogger, string username, string email) {
        List<string> admins = ["C6OI"];

        Logger = connectionLogger.ForType(typeof(Player));
        Username = username;
        Email = email;

        if (admins.Contains(Username))
            Groups |= PlayerGroups.Admin;
    }

    public ILogger Logger { get; }

    public string Username { get; set; }
    public string Email { get; set; }

    public bool RememberMe { get; set; }

    public byte[] AutoLoginToken { get; set; }
    public byte[] PasswordHash { get; set; }
    public string HardwareFingerprint { get; set; } = null!;

    public PlayerGroups Groups { get; set; }
    public bool IsAdmin => (Groups & PlayerGroups.Admin) == PlayerGroups.Admin;
    public bool IsModerator => IsAdmin || (Groups & PlayerGroups.Moderator) == PlayerGroups.Moderator;
    public bool IsTester => (Groups & PlayerGroups.Tester) == PlayerGroups.Tester;
    public bool IsPremium => (Groups & PlayerGroups.Premium) == PlayerGroups.Premium;
    public bool IsBanned => (Groups & PlayerGroups.Banned) == PlayerGroups.Banned;

    public bool Subscribed { get; set; }
    public string CountryCode { get; set; } = "XD";

    public long Crystals { get; set; }
    public long XCrystals { get; set; }

    public int GoldBoxItems { get; set; }

    public long Experience { get; set; }

    public DateTimeOffset RegistrationTime { get; set; }
    public DateTimeOffset LastLoginTime { get; set; }

    public HashSet<long> AcceptedFriendIds { get; set; } = [];
    public HashSet<long> IncomingFriendIds { get; set; } = [];
    public HashSet<long> OutgoingFriendIds { get; set; } = [];

    public HashSet<long> BlockedPlayerIds { get; set; } = [];
    public HashSet<long> ReportedPlayerIds { get; set; } = [];
}

[Flags]
public enum PlayerGroups {
    None = 0,
    Admin = 1,
    Moderator = 2,
    Tester = 4,
    Premium = 8,
    Banned = 16
}

using System.ComponentModel.DataAnnotations.Schema;
using Serilog;
using Vint.Core.Utils;

namespace Vint.Core.ECS;

public class Player {
    public Player(ILogger connectionLogger, string username, string email) : this(username, email) =>
        Logger = connectionLogger.ForType(typeof(Player));

    Player(string username, string email) {
        List<string> admins = ["C6OI"];

        Logger = Log.Logger.ForType(typeof(Player));
        Username = username;
        Email = email;

        if (admins.Contains(Username))
            Groups |= PlayerGroups.Admin;
    }

    public ILogger Logger { get; }

    public uint Id { get; private init; }

    public string Username { get; set; }
    public string Email { get; set; }

    public bool RememberMe { get; set; }

    public byte[] AutoLoginToken { get; set; } = [];
    public byte[] PasswordHash { get; set; } = [];
    public string HardwareFingerprint { get; set; } = "";

    public PlayerGroups Groups { get; set; }
    public bool IsAdmin => (Groups & PlayerGroups.Admin) == PlayerGroups.Admin;
    public bool IsModerator => IsAdmin || (Groups & PlayerGroups.Moderator) == PlayerGroups.Moderator;
    public bool IsTester => (Groups & PlayerGroups.Tester) == PlayerGroups.Tester;
    public bool IsPremium => (Groups & PlayerGroups.Premium) == PlayerGroups.Premium;
    public bool IsBanned => (Groups & PlayerGroups.Banned) == PlayerGroups.Banned;

    public bool Subscribed { get; set; }
    public string CountryCode { get; set; } = "RU";

    public long Crystals { get; set; }
    public long XCrystals { get; set; }

    public int GoldBoxItems { get; set; }

    public long Experience { get; set; }
    public int Rank => Leveling.GetRank(Experience);

    public DateTimeOffset RegistrationTime { get; init; }
    public DateTimeOffset LastLoginTime { get; set; }

    public List<long> AcceptedFriendIds { get; set; } = [];
    public List<long> IncomingFriendIds { get; set; } = [];
    public List<long> OutgoingFriendIds { get; set; } = [];

    public List<long> BlockedPlayerIds { get; set; } = [];
    public List<long> ReportedPlayerIds { get; set; } = [];
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

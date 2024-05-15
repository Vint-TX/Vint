using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Vint.Core.Utils;

public static class TimeSpanUtils {
    static Dictionary<string, TimeSpan> TimeSpanInfos { get; } = new() {
        { "year", FromYears(1) },
        { "month", TimeSpan.FromDays(30) },
        { "week", TimeSpan.FromDays(7) },
        { "day", TimeSpan.FromDays(1) },
        { "hour", TimeSpan.FromHours(1) },
        { "min", TimeSpan.FromMinutes(1) },
        { "sec", TimeSpan.FromSeconds(1) }
    };

    public static bool TryParseDuration(string? format, [NotNullWhen(true)] out TimeSpan? duration) {
        if (string.IsNullOrWhiteSpace(format)) {
            duration = null;
            return false;
        }

        TimeSpan result = new();

        try {
            foreach ((string abbreviation, TimeSpan initialTimeSpan) in TimeSpanInfos) {
                if (!format.Contains(abbreviation)) continue;

                result += initialTimeSpan * int.Parse(new Regex(@$"(\d+){abbreviation}").Match(format).Groups[1].Value);
            }
        } catch {
            duration = null;
            return false;
        }

        duration = result;
        return true;
    }

    public static TimeSpan FromYears(double value) {
        int leapCounter = 0;
        double years = value;

        while (years >= 4) {
            leapCounter++;
            years -= 4;
        }

        return TimeSpan.FromDays(value * 365 + leapCounter);
    }
}

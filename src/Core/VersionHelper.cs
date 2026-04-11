using System.Text.RegularExpressions;

namespace DigitalScope.Core;

public static partial class VersionHelper
{
    public static bool IsRemoteVersionNewer(string? currentVersion, string? remoteVersion)
        => CompareReleaseVersions(currentVersion, remoteVersion) < 0;

    public static int CompareReleaseVersions(string? leftVersion, string? rightVersion)
    {
        var left  = Parse(leftVersion);
        var right = Parse(rightVersion);

        int semverCompare = CompareSegments(left.SemVerParts, right.SemVerParts);
        if (semverCompare != 0)
            return semverCompare;

        if (!string.IsNullOrWhiteSpace(left.BuildCode) && !string.IsNullOrWhiteSpace(right.BuildCode))
        {
            int buildCompare = CompareBuildCodes(left.BuildCode, right.BuildCode);
            if (buildCompare != 0)
                return buildCompare;
        }

        return string.Compare(left.Normalized, right.Normalized, StringComparison.OrdinalIgnoreCase);
    }

    public static string ToDisplayVersion(string? version)
    {
        var parsed = Parse(version);
        if (parsed.SemVerParts.Length == 0)
            return string.IsNullOrWhiteSpace(parsed.Normalized) ? "Unknown" : parsed.Normalized;

        var versionCore = string.Join('.', parsed.SemVerParts);
        return string.IsNullOrWhiteSpace(parsed.BuildCode)
            ? versionCore
            : $"{versionCore}.{parsed.BuildCode}";
    }

    private static ParsedVersion Parse(string? version)
    {
        var normalized = (version ?? string.Empty).Trim();
        normalized = normalized.TrimStart('v', 'V');

        var tokens = VersionTokenRegex()
            .Matches(normalized)
            .Select(match => match.Value)
            .ToArray();

        var semverParts = tokens
            .Take(3)
            .Select(token => int.TryParse(token, out var part) ? part : 0)
            .ToArray();

        string? buildCode = tokens
            .Skip(3)
            .FirstOrDefault(token => token.Length >= 8)
            ?? tokens.Skip(3).FirstOrDefault();

        return new ParsedVersion(normalized, semverParts, buildCode);
    }

    private static int CompareSegments(IReadOnlyList<int> left, IReadOnlyList<int> right)
    {
        int length = Math.Max(left.Count, right.Count);
        for (int i = 0; i < length; i++)
        {
            int a = i < left.Count ? left[i] : 0;
            int b = i < right.Count ? right[i] : 0;
            if (a != b)
                return a.CompareTo(b);
        }
        return 0;
    }

    private static int CompareBuildCodes(string left, string right)
    {
        var a = ParseBuildCode(left);
        var b = ParseBuildCode(right);

        int yearCompare = a.Year.CompareTo(b.Year);
        if (yearCompare != 0) return yearCompare;

        int monthCompare = a.Month.CompareTo(b.Month);
        if (monthCompare != 0) return monthCompare;

        int dayCompare = a.Day.CompareTo(b.Day);
        if (dayCompare != 0) return dayCompare;

        return a.Sequence.CompareTo(b.Sequence);
    }

    private static BuildCodeParts ParseBuildCode(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length < 8)
        {
            return new BuildCodeParts(
                Year: 0, Month: 0, Day: 0,
                Sequence: int.TryParse(digits, out var seqFallback) ? seqFallback : 0);
        }

        int day      = ParseInt(digits[..2]);
        int month    = ParseInt(digits.Substring(2, 2));
        int year     = ParseInt(digits.Substring(4, 4));
        int sequence = digits.Length > 8 ? ParseInt(digits[8..]) : 0;

        return new BuildCodeParts(year, month, day, sequence);
    }

    private static int ParseInt(string value)
        => int.TryParse(value, out var parsed) ? parsed : 0;

    [GeneratedRegex(@"\d+")]
    private static partial Regex VersionTokenRegex();

    private sealed record ParsedVersion(string Normalized, int[] SemVerParts, string? BuildCode);
    private sealed record BuildCodeParts(int Year, int Month, int Day, int Sequence);
}

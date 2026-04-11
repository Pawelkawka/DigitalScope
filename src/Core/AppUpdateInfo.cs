using System.Text.Json.Serialization;

namespace DigitalScope.Core;

public sealed record AppUpdateInfo(
    string TagName,
    string DisplayVersion,
    string ReleaseNotes,
    string ReleaseUrl);

internal sealed record GitHubReleaseResponse(
    [property: JsonPropertyName("tag_name")] string? TagName,
    [property: JsonPropertyName("body")] string? Body,
    [property: JsonPropertyName("html_url")] string? HtmlUrl);

using System.Net.Http;
using System.Net.Http.Json;

namespace DigitalScope.Core;

public sealed class AppUpdateService
{
    private static readonly HttpClient Http = CreateHttpClient();

    public async Task<AppUpdateInfo?> CheckForUpdateAsync(CancellationToken ct = default)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2), ct);

            using var request = new HttpRequestMessage(HttpMethod.Get, AppSettings.ReleaseApiUrl);
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            request.Headers.UserAgent.ParseAdd($"{AppSettings.AppName}/{AppSettings.AppVersion}");

            using var response = await Http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                AppLogger.Warn($"Updater HTTP {(int)response.StatusCode} while checking releases.");
                return null;
            }

            var release = await response.Content.ReadFromJsonAsync<GitHubReleaseResponse>(cancellationToken: ct);
            if (release is null || string.IsNullOrWhiteSpace(release.TagName))
            {
                AppLogger.Warn("Updater returned an empty release payload.");
                return null;
            }

            if (!VersionHelper.IsRemoteVersionNewer(AppSettings.AppVersion, release.TagName))
            {
                AppLogger.Info("Updater: application is up to date.");
                return null;
            }

            var updateInfo = new AppUpdateInfo(
                TagName: release.TagName,
                DisplayVersion: VersionHelper.ToDisplayVersion(release.TagName),
                ReleaseNotes: release.Body ?? string.Empty,
                ReleaseUrl: string.IsNullOrWhiteSpace(release.HtmlUrl)
                    ? AppSettings.ReleasesPageUrl
                    : release.HtmlUrl);

            AppLogger.Info($"Updater: found new version {updateInfo.DisplayVersion}.");
            return updateInfo;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            AppLogger.Warn($"Updater: {ex.Message}");
            return null;
        }
    }

    private static HttpClient CreateHttpClient()
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10),
        };
    }
}

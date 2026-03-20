// GitHub Releases update client.
// Replaces the legacy SOAP proxy (audioswit.ch) with the GitHub releases API.

using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FortyOne.AudioSwitcher.AudioSwitcherService
{
    public class AudioSwitcherVersionInfo
    {
        public string VersionInfo { get; set; }
        public string URL { get; set; }
        public string ChangeLog { get; set; }
    }

    public sealed class AudioSwitcherClient : IDisposable
    {
        private const string GitHubApiUrl =
            "https://api.github.com/repos/jrgrafisk/AudioSwitcher_v1/releases/latest";

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        static AudioSwitcherClient()
        {
            // GitHub API requires a User-Agent header.
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "AudioSwitcher");

            // Ask GitHub for JSON back.
            if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        }

        /// <summary>
        /// Fetches the latest GitHub release and returns version info if a newer
        /// version than <paramref name="assemblyVersion"/> is available.
        /// Returns null if already up-to-date or on any error.
        /// </summary>
        public AudioSwitcherVersionInfo GetUpdateInfo(string assemblyVersion)
        {
            return GetUpdateInfoAsync(assemblyVersion).GetAwaiter().GetResult();
        }

        public async Task<AudioSwitcherVersionInfo> GetUpdateInfoAsync(string assemblyVersion)
        {
            try
            {
                var json = await _httpClient.GetStringAsync(GitHubApiUrl).ConfigureAwait(false);

                var tagName   = ParseJsonString(json, "tag_name");
                var htmlUrl   = ParseJsonString(json, "html_url");
                var body      = ParseJsonString(json, "body");

                if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(htmlUrl))
                    return null;

                if (!IsNewer(tagName, assemblyVersion))
                    return null;

                return new AudioSwitcherVersionInfo
                {
                    VersionInfo = tagName.TrimStart('v', 'V'),
                    URL         = htmlUrl,
                    ChangeLog   = body ?? ""
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the release page URL if an update is available, null otherwise.
        /// </summary>
        public string CheckForUpdate(string assemblyVersion)
        {
            return CheckForUpdateAsync(assemblyVersion).GetAwaiter().GetResult();
        }

        public async Task<string> CheckForUpdateAsync(string assemblyVersion)
        {
            var info = await GetUpdateInfoAsync(assemblyVersion).ConfigureAwait(false);
            return info?.URL;
        }

        // ── helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if <paramref name="githubTag"/> represents a version
        /// strictly newer than <paramref name="assemblyVersion"/>.
        /// </summary>
        private static bool IsNewer(string githubTag, string assemblyVersion)
        {
            // Strip leading 'v' or 'V' from tag (e.g. "v1.2.3" → "1.2.3")
            var tagVersion = githubTag.TrimStart('v', 'V');

            if (!Version.TryParse(tagVersion, out Version latest))
                return false;

            // AssemblyVersion may be "1.2.3.4" — normalise to three parts for comparison.
            var normalized = NormalizeVersion(assemblyVersion);
            if (!Version.TryParse(normalized, out Version current))
                return false;

            return latest > current;
        }

        /// <summary>
        /// Trims a version string to at most three components so it can be
        /// compared with a typical GitHub semver tag (major.minor.patch).
        /// </summary>
        private static string NormalizeVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return "0.0.0";
            var parts = version.Split('.');
            var take = Math.Min(parts.Length, 3);
            return string.Join(".", parts, 0, take);
        }

        /// <summary>
        /// Extracts a simple JSON string value by key using regex.
        /// Handles basic JSON escape sequences.
        /// </summary>
        private static string ParseJsonString(string json, string key)
        {
            // Matches: "key": "value"  (value may contain \" escapes and \n etc.)
            var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"";
            var m = Regex.Match(json, pattern, RegexOptions.Singleline);
            if (!m.Success) return null;

            return Regex.Unescape(m.Groups[1].Value);
        }

        public void Dispose() { }
    }
}

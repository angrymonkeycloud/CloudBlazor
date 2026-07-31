using System.Globalization;
using System.Text;

namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// Builds a <c>robots.txt</c> body.
/// </summary>
/// <remarks>
/// Distinct from the per-page robots <em>meta tag</em>: this file governs crawling (whether a
/// URL is fetched at all), while the meta tag governs indexing of a page that was fetched.
/// A URL disallowed here can still be indexed from external links, so use
/// <c>SetIndexPage(false)</c> to keep a page out of an index.
/// </remarks>
public class CloudRobotsFile
{
    private readonly List<CloudRobotsGroup> _groups = [];
    private readonly List<string> _sitemaps = [];

    /// <summary>The user-agent groups added so far.</summary>
    public IReadOnlyList<CloudRobotsGroup> Groups => _groups;

    /// <summary>The sitemap URLs advertised by this file.</summary>
    public IReadOnlyList<string> Sitemaps => _sitemaps;

    /// <summary>
    /// Adds an <c>Allow</c> rule, creating the user-agent group when it does not exist yet.
    /// </summary>
    /// <param name="path">Path prefix, such as <c>/</c>.</param>
    /// <param name="userAgent">Target crawler, or <c>*</c> for all of them.</param>
    public CloudRobotsFile Allow(string path, string userAgent = "*")
    {
        Group(userAgent).Allowed.Add(path);

        return this;
    }

    /// <summary>
    /// Adds a <c>Disallow</c> rule, creating the user-agent group when it does not exist yet.
    /// </summary>
    /// <param name="path">Path prefix, such as <c>/admin</c>.</param>
    /// <param name="userAgent">Target crawler, or <c>*</c> for all of them.</param>
    public CloudRobotsFile Disallow(string path, string userAgent = "*")
    {
        Group(userAgent).Disallowed.Add(path);

        return this;
    }

    /// <summary>Sets a crawl delay in seconds for a user agent.</summary>
    public CloudRobotsFile CrawlDelay(double seconds, string userAgent = "*")
    {
        Group(userAgent).CrawlDelay = seconds;

        return this;
    }

    /// <summary>Advertises a sitemap URL.</summary>
    public CloudRobotsFile AddSitemap(string sitemapUrl)
    {
        if (!string.IsNullOrWhiteSpace(sitemapUrl) && !_sitemaps.Contains(sitemapUrl, StringComparer.OrdinalIgnoreCase))
            _sitemaps.Add(sitemapUrl);

        return this;
    }

    /// <summary>Blocks every crawler from the whole site.</summary>
    public CloudRobotsFile DisallowAll()
    {
        _groups.Clear();

        return Disallow("/");
    }

    private CloudRobotsGroup Group(string userAgent)
    {
        CloudRobotsGroup? group = _groups.FirstOrDefault(candidate =>
            string.Equals(candidate.UserAgent, userAgent, StringComparison.OrdinalIgnoreCase));

        if (group == null)
        {
            group = new CloudRobotsGroup { UserAgent = userAgent };
            _groups.Add(group);
        }

        return group;
    }

    /// <summary>
    /// Renders the file. With no groups configured, everything is allowed — the permissive
    /// default a missing <c>robots.txt</c> already implies, stated explicitly.
    /// </summary>
    /// <param name="baseUrl">Origin used to make relative sitemap URLs absolute.</param>
    public string ToFileContent(string? baseUrl = null)
    {
        StringBuilder builder = new();

        IReadOnlyList<CloudRobotsGroup> groups = _groups.Count > 0
            ? _groups
            : [new CloudRobotsGroup { UserAgent = "*", Allowed = { "/" } }];

        foreach (CloudRobotsGroup group in groups)
        {
            if (builder.Length > 0)
                builder.AppendLine();

            builder.AppendLine($"User-agent: {group.UserAgent}");

            foreach (string path in group.Allowed)
                builder.AppendLine($"Allow: {path}");

            foreach (string path in group.Disallowed)
                builder.AppendLine($"Disallow: {path}");

            if (group.CrawlDelay.HasValue)
                builder.AppendLine($"Crawl-delay: {group.CrawlDelay.Value.ToString("0.###", CultureInfo.InvariantCulture)}");
        }

        if (_sitemaps.Count > 0)
        {
            builder.AppendLine();

            foreach (string sitemap in _sitemaps)
                builder.AppendLine($"Sitemap: {CloudPage.ToAbsolute(sitemap, baseUrl) ?? sitemap}");
        }

        return builder.ToString();
    }
}

/// <summary>
/// The rules that apply to one user agent in a <c>robots.txt</c> file.
/// </summary>
public class CloudRobotsGroup
{
    /// <summary>Target crawler, or <c>*</c> for all of them.</summary>
    public required string UserAgent { get; set; }

    /// <summary>Path prefixes the crawler may fetch.</summary>
    public List<string> Allowed { get; init; } = [];

    /// <summary>Path prefixes the crawler must not fetch.</summary>
    public List<string> Disallowed { get; init; } = [];

    /// <summary>Seconds the crawler should wait between requests.</summary>
    public double? CrawlDelay { get; set; }
}

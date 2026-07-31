namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// One <c>&lt;url&gt;</c> in a sitemap.
/// </summary>
public class CloudSitemapEntry
{
    /// <summary>
    /// Absolute URL, or a site-relative path resolved against the request origin when the
    /// sitemap renders.
    /// </summary>
    public required string Location { get; set; }

    /// <summary>When the page last changed.</summary>
    public DateTimeOffset? LastModified { get; set; }

    /// <summary>How often the page is expected to change.</summary>
    public CloudChangeFrequencies? ChangeFrequency { get; set; }

    /// <summary>
    /// Relative importance within this site, from <c>0.0</c> to <c>1.0</c>. Compares pages
    /// against each other on the same site; it carries no meaning across sites.
    /// </summary>
    public double? Priority { get; set; }

    /// <summary>
    /// Localized variants of this URL, rendered as <c>xhtml:link</c> elements so every
    /// language in a set is discoverable from any one of them.
    /// </summary>
    public IReadOnlyList<CloudAlternateLink> Alternates { get; set; } = [];
}

namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// Describes how a site's languages map onto its URLs, so canonical links, <c>hreflang</c>
/// alternates and <c>og:locale</c> can be derived instead of repeated on every page.
/// </summary>
/// <remarks>
/// <para>
/// Assumes the common convention: the default language lives at <c>/path</c> and every other
/// language at <c>/{culture}/path</c>. A page that sets its own canonical or alternates keeps
/// them — the derived values only fill gaps.
/// </para>
/// <para>
/// Without this, every multilingual application ends up writing the same path arithmetic in a
/// wrapper component, and getting the reciprocal alternate set subtly wrong.
/// </para>
/// </remarks>
public class CloudLocalizationOptions
{
    /// <summary>
    /// The language served from unprefixed URLs, and the one advertised as <c>x-default</c>.
    /// </summary>
    public required string DefaultCulture { get; set; }

    /// <summary>
    /// Every language the site serves, including <see cref="DefaultCulture"/>.
    /// </summary>
    public required IReadOnlyList<string> SupportedCultures { get; set; }

    /// <summary>
    /// Open Graph locale per culture, such as <c>["en"] = "en_US"</c>. Cultures with no entry
    /// fall back to the culture name with <c>-</c> replaced by <c>_</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string> Locales { get; set; } = new Dictionary<string, string>();

    /// <summary>Derive the canonical URL when a page does not set one.</summary>
    public bool AutoCanonical { get; set; } = true;

    /// <summary>Derive the <c>hreflang</c> set when a page does not set one.</summary>
    public bool AutoAlternates { get; set; } = true;

    /// <summary>Derive <c>og:locale</c> and its alternates when a page does not set them.</summary>
    public bool AutoLocale { get; set; } = true;

    /// <summary>
    /// Indicates whether a path segment names one of the supported cultures.
    /// </summary>
    public bool IsCultureSegment(string? segment) =>
        !string.IsNullOrWhiteSpace(segment)
        && SupportedCultures.Any(culture => string.Equals(culture, segment, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Strips the culture prefix from a request path, leaving the culture-neutral remainder.
    /// </summary>
    /// <returns>The neutral path without leading or trailing slashes.</returns>
    public string NeutralPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        int cut = path.IndexOfAny(['?', '#']);

        if (cut >= 0)
            path = path[..cut];

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length > 0 && IsCultureSegment(segments[0]))
            segments = [.. segments.Skip(1)];

        return string.Join('/', segments);
    }

    /// <summary>
    /// The culture a request path is served in, or <see cref="DefaultCulture"/> when the path
    /// carries no culture prefix.
    /// </summary>
    public string CultureOf(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return DefaultCulture;

        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0 || !IsCultureSegment(segments[0]))
            return DefaultCulture;

        return SupportedCultures.First(culture => string.Equals(culture, segments[0], StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The site-relative URL for a neutral path in a given culture.
    /// </summary>
    public string PathFor(string culture, string neutralPath)
    {
        string trimmed = neutralPath.Trim('/');
        bool isDefault = string.Equals(culture, DefaultCulture, StringComparison.OrdinalIgnoreCase);

        if (isDefault)
            return trimmed.Length == 0 ? "/" : $"/{trimmed}";

        return trimmed.Length == 0 ? $"/{culture}" : $"/{culture}/{trimmed}";
    }

    /// <summary>
    /// The complete reciprocal <c>hreflang</c> set for a neutral path, including
    /// <c>x-default</c>.
    /// </summary>
    public IEnumerable<CloudAlternateLink> AlternatesFor(string neutralPath)
    {
        foreach (string culture in SupportedCultures)
            yield return new CloudAlternateLink(culture, PathFor(culture, neutralPath));

        yield return new CloudAlternateLink(CloudAlternateLink.XDefault, PathFor(DefaultCulture, neutralPath));
    }

    /// <summary>The Open Graph locale for a culture.</summary>
    public string LocaleFor(string culture) =>
        Locales.TryGetValue(culture, out string? locale) ? locale : culture.Replace('-', '_');
}

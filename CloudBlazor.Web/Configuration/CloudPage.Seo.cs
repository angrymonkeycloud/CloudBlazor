using System.Collections.ObjectModel;
using System.Text.Json;

namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// Discovery and sharing metadata: canonical URL, language alternates, Open Graph,
/// Twitter cards and JSON-LD structured data.
/// </summary>
/// <remarks>
/// Kept apart from the core head metadata so each file stays readable; it is the same
/// fluent builder, and every setter raises <c>OnModified</c> like the rest.
/// </remarks>
public partial class CloudPage
{
    // ── Canonical and alternates ──────────────────────────────────────────

    /// <summary>Canonical URL of this page.</summary>
    public string? Canonical { get; internal set; }

    internal readonly List<CloudAlternateLink> _alternates = [];

    /// <summary>Localized variants of this page.</summary>
    public ReadOnlyCollection<CloudAlternateLink> Alternates => _alternates.AsReadOnly();

    // ── Open Graph ────────────────────────────────────────────────────────

    /// <summary>Open Graph object type, such as <c>website</c> or <c>article</c>.</summary>
    public string? OpenGraphType { get; internal set; }

    /// <summary>Site name shown alongside the page title in a link preview.</summary>
    public string? SiteName { get; internal set; }

    /// <summary>Title for link previews. Falls back to the page title.</summary>
    public string? SocialTitle { get; internal set; }

    /// <summary>Description for link previews. Falls back to the meta description.</summary>
    public string? SocialDescription { get; internal set; }

    /// <summary>Preview image shared by Open Graph and Twitter cards.</summary>
    public CloudPageImage? Image { get; internal set; }

    /// <summary>Locale of this page, in Open Graph's <c>en_US</c> form.</summary>
    public string? Locale { get; internal set; }

    internal readonly List<string> _localeAlternates = [];

    /// <summary>Other locales this page is available in.</summary>
    public ReadOnlyCollection<string> LocaleAlternates => _localeAlternates.AsReadOnly();

    // ── Twitter ───────────────────────────────────────────────────────────

    /// <summary>Twitter card layout. Defaults to <see cref="CloudTwitterCards.SummaryLargeImage"/> when an image is set.</summary>
    public CloudTwitterCards? TwitterCard { get; internal set; }

    /// <summary>The <c>@handle</c> of the site.</summary>
    public string? TwitterSite { get; internal set; }

    /// <summary>The <c>@handle</c> of the content author.</summary>
    public string? TwitterCreator { get; internal set; }

    // ── Structured data ───────────────────────────────────────────────────

    internal readonly List<string> _structuredData = [];

    /// <summary>Serialized JSON-LD documents rendered as <c>application/ld+json</c> scripts.</summary>
    public ReadOnlyCollection<string> StructuredData => _structuredData.AsReadOnly();

    private static readonly JsonSerializerOptions _structuredDataOptions = new()
    {
        // Allowing all Unicode keeps non-Latin content (Arabic, Chinese) readable instead of
        // tripling in size as \uXXXX escapes. Deliberately not UnsafeRelaxedJsonEscaping:
        // that leaves '<' intact, and this JSON is written inside a <script> element, so a
        // value containing "</script>" would close the tag and execute what follows.
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // ── Setters ───────────────────────────────────────────────────────────

    /// <summary>Sets the canonical URL, the address search engines should treat as authoritative.</summary>
    public CloudPage SetCanonical(string canonical)
    {
        Canonical = canonical;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Adds a localized variant of this page.</summary>
    /// <param name="hrefLang">Language code, or <see cref="CloudAlternateLink.XDefault"/>.</param>
    /// <param name="href">Absolute URL of the variant.</param>
    public CloudPage AddAlternate(string hrefLang, string href) => AddAlternates(new CloudAlternateLink(hrefLang, href));

    /// <summary>Adds localized variants of this page.</summary>
    public CloudPage AddAlternates(params CloudAlternateLink[]? alternates)
    {
        if (alternates == null)
            return this;

        // A repeated hreflang is contradictory rather than additive, so the last one wins.
        foreach (CloudAlternateLink alternate in alternates)
        {
            _alternates.RemoveAll(existing => string.Equals(existing.HrefLang, alternate.HrefLang, StringComparison.OrdinalIgnoreCase));
            _alternates.Add(alternate);
        }

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Replaces every language alternate.</summary>
    public CloudPage SetAlternates(IEnumerable<CloudAlternateLink> alternates)
    {
        _alternates.Clear();
        _alternates.AddRange(alternates);

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the Open Graph object type. Defaults to <c>website</c>.</summary>
    public CloudPage SetOpenGraphType(string openGraphType)
    {
        OpenGraphType = openGraphType;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the site name shown in link previews.</summary>
    public CloudPage SetSiteName(string siteName)
    {
        SiteName = siteName;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Overrides the link-preview title, which otherwise follows the page title.</summary>
    public CloudPage SetSocialTitle(string socialTitle)
    {
        SocialTitle = socialTitle;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Overrides the link-preview description, which otherwise follows the meta description.</summary>
    public CloudPage SetSocialDescription(string socialDescription)
    {
        SocialDescription = socialDescription;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the link-preview image by URL.</summary>
    public CloudPage SetImage(string url) => SetImage(new CloudPageImage { Url = url });

    /// <summary>Sets the link-preview image, with optional dimensions and alternative text.</summary>
    public CloudPage SetImage(CloudPageImage image)
    {
        Image = image;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets this page's locale, in Open Graph's <c>en_US</c> form.</summary>
    public CloudPage SetLocale(string locale)
    {
        Locale = locale;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Adds locales this page is also available in.</summary>
    public CloudPage AddLocaleAlternates(params string[]? locales)
    {
        if (locales == null)
            return this;

        foreach (string locale in locales)
            if (!_localeAlternates.Contains(locale, StringComparer.OrdinalIgnoreCase))
                _localeAlternates.Add(locale);

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the Twitter card layout.</summary>
    public CloudPage SetTwitterCard(CloudTwitterCards twitterCard)
    {
        TwitterCard = twitterCard;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the site's Twitter <c>@handle</c>.</summary>
    public CloudPage SetTwitterSite(string twitterSite)
    {
        TwitterSite = twitterSite;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the author's Twitter <c>@handle</c>.</summary>
    public CloudPage SetTwitterCreator(string twitterCreator)
    {
        TwitterCreator = twitterCreator;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Adds a JSON-LD document, supplied as already-serialized JSON.</summary>
    public CloudPage AddStructuredData(string json)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            _structuredData.Add(json);

            OnModified?.Invoke();
        }

        return this;
    }

    /// <summary>
    /// Adds a JSON-LD document by serializing an object. Anonymous types and dictionaries
    /// both work, which keeps <c>@context</c> and <c>@type</c> expressible without a
    /// schema.org type library.
    /// </summary>
    public CloudPage AddStructuredData(object structuredData)
    {
        ArgumentNullException.ThrowIfNull(structuredData);

        return AddStructuredData(JsonSerializer.Serialize(structuredData, _structuredDataOptions));
    }

    /// <summary>Removes every structured-data document.</summary>
    public CloudPage ClearStructuredData()
    {
        _structuredData.Clear();

        OnModified?.Invoke();

        return this;
    }

    // ── Results ───────────────────────────────────────────────────────────

    /// <summary>The canonical URL to render, absolute where <paramref name="baseUrl"/> allows.</summary>
    public string? CanonicalResult(string? baseUrl = null) => ToAbsolute(Canonical, baseUrl);

    /// <summary>Language alternates with their URLs made absolute where possible.</summary>
    public IEnumerable<CloudAlternateLink> AlternatesResult(string? baseUrl = null) =>
        _alternates.Select(alternate => alternate with { Href = ToAbsolute(alternate.Href, baseUrl) ?? alternate.Href });

    /// <summary>
    /// The structured-data documents, made safe to write inside a <c>&lt;script&gt;</c> element.
    /// </summary>
    /// <remarks>
    /// Every <c>&lt;</c> becomes <c><</c>. In well-formed JSON that character only ever
    /// occurs inside a string literal, where the escape is equivalent, so the document still
    /// parses identically — but <c>&lt;/script&gt;</c> can no longer terminate the element.
    /// This matters for documents supplied as raw JSON through
    /// <see cref="AddStructuredData(string)"/>, which never passed through the serializer.
    /// </remarks>
    public IEnumerable<string> StructuredDataResult() =>
        _structuredData.Select(static document => document.Replace("<", "\\u003C", StringComparison.Ordinal));

    /// <summary>The Open Graph object type, defaulting to <c>website</c>.</summary>
    public string OpenGraphTypeResult() => string.IsNullOrWhiteSpace(OpenGraphType) ? "website" : OpenGraphType;

    /// <summary>
    /// The link-preview title: the explicit social title when set, otherwise the page title.
    /// </summary>
    /// <remarks>
    /// Deliberately the raw <see cref="CloudPage.Title"/> rather than the composed
    /// <c>TitleResult</c>: a site-wide suffix reads as noise in a shared link, where the
    /// site name is already carried by <c>og:site_name</c>.
    /// </remarks>
    public string? SocialTitleResult() => string.IsNullOrWhiteSpace(SocialTitle) ? Title : SocialTitle;

    /// <summary>
    /// The link-preview description: the explicit social description when set, otherwise the
    /// meta description. Not truncated — previews allow more text than a search snippet.
    /// </summary>
    public string? SocialDescriptionResult() => string.IsNullOrWhiteSpace(SocialDescription) ? Description : SocialDescription;

    /// <summary>The preview image with its URL made absolute where possible.</summary>
    public CloudPageImage? ImageResult(string? baseUrl = null)
    {
        if (Image == null)
            return null;

        string? absolute = ToAbsolute(Image.Url, baseUrl);

        if (absolute == Image.Url)
            return Image;

        return new CloudPageImage
        {
            Url = absolute ?? Image.Url,
            Width = Image.Width,
            Height = Image.Height,
            Alt = Image.Alt,
            MimeType = Image.MimeType
        };
    }

    /// <summary>
    /// The Twitter card layout: the explicit value, otherwise a large-image card when the
    /// page has an image and a plain summary when it does not.
    /// </summary>
    public CloudTwitterCards TwitterCardResult() =>
        TwitterCard ?? (Image == null ? CloudTwitterCards.Summary : CloudTwitterCards.SummaryLargeImage);

    /// <summary>The <c>twitter:card</c> token for <see cref="TwitterCardResult"/>.</summary>
    public string TwitterCardValueResult() => TwitterCardResult() switch
    {
        CloudTwitterCards.SummaryLargeImage => "summary_large_image",
        CloudTwitterCards.App => "app",
        CloudTwitterCards.Player => "player",
        _ => "summary"
    };

    /// <summary>
    /// Resolves a possibly-relative URL against <paramref name="baseUrl"/>. Values that are
    /// already absolute pass through, and a relative value with no base is returned as-is
    /// rather than dropped, so a same-origin canonical still renders.
    /// </summary>
    internal static string? ToAbsolute(string? url, string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            return absolute.ToString();

        if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? root))
            return url;

        return new Uri(root, url).ToString();
    }
}

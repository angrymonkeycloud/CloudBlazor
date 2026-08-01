using System.Globalization;

namespace AngryMonkey.CloudBlazor;

/// <summary>
/// Describes a document-head <c>link</c> element without depending on ASP.NET Core.
/// It can be shared by server-rendered, WebAssembly, and hybrid Blazor hosts.
/// </summary>
public sealed record CloudHeadLink
{
    public required string Rel { get; init; }
    public required string Href { get; init; }
    public string? As { get; init; }
    public string? Type { get; init; }
    public string? Sizes { get; init; }
    public string? Media { get; init; }
    public string? CrossOrigin { get; init; }
    public string? FetchPriority { get; init; }

    /// <summary>
    /// Optional language-specific hrefs keyed by a culture name such as <c>ar</c> or
    /// <c>ar-LB</c>. The full UI culture is checked before its neutral language.
    /// </summary>
    public IReadOnlyDictionary<string, string>? LocalizedHrefs { get; init; }

    public string HrefResult(CultureInfo? culture = null)
    {
        if (LocalizedHrefs == null || LocalizedHrefs.Count == 0)
            return Href;

        culture ??= CultureInfo.CurrentUICulture;

        if (LocalizedHrefs.TryGetValue(culture.Name, out string? exact))
            return exact;

        if (LocalizedHrefs.TryGetValue(culture.TwoLetterISOLanguageName, out string? neutral))
            return neutral;

        return Href;
    }

    /// <summary>Returns the explicitly configured MIME type, or infers it from the resolved URL.</summary>
    public string? TypeResult(CultureInfo? culture = null)
    {
        if (!string.IsNullOrWhiteSpace(Type))
            return Type;

        string path = HrefResult(culture).Split('?', '#')[0];
        string extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".ico" => "image/x-icon",
            ".svg" => "image/svg+xml",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".avif" => "image/avif",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".ttf" => "font/ttf",
            ".otf" => "font/otf",
            ".css" => "text/css",
            ".js" or ".mjs" => "text/javascript",
            ".json" or ".webmanifest" => "application/manifest+json",
            _ => null
        };
    }

    public static CloudHeadLink Icon(string href, string? type = null, string? sizes = null) => new()
    {
        Rel = "icon",
        Href = href,
        Type = type,
        Sizes = sizes
    };

    public static CloudHeadLink AppleTouchIcon(string href, string? sizes = null) => new()
    {
        Rel = "apple-touch-icon",
        Href = href,
        Sizes = sizes
    };

    public static CloudHeadLink Preload(
        string href,
        string asType,
        string? type = null,
        string? crossOrigin = null,
        string? fetchPriority = null) => new()
    {
        Rel = "preload",
        Href = href,
        As = asType,
        Type = type,
        CrossOrigin = crossOrigin,
        FetchPriority = fetchPriority
    };

    public static CloudHeadLink LocalizedPreload(
        string fallbackHref,
        IReadOnlyDictionary<string, string> localizedHrefs,
        string asType,
        string? type = null,
        string? crossOrigin = null,
        string? fetchPriority = null) => Preload(fallbackHref, asType, type, crossOrigin, fetchPriority) with
    {
        LocalizedHrefs = localizedHrefs
    };

    /// <summary>Preloads a font and infers its MIME type from the URL.</summary>
    public static CloudHeadLink FontPreload(string href, string crossOrigin = "anonymous") =>
        Preload(href, "font", crossOrigin: crossOrigin);

    /// <summary>Preloads only the font used by the current UI language.</summary>
    public static CloudHeadLink LocalizedFontPreload(
        string fallbackHref,
        IReadOnlyDictionary<string, string> localizedHrefs,
        string crossOrigin = "anonymous") =>
        LocalizedPreload(fallbackHref, localizedHrefs, "font", crossOrigin: crossOrigin);

    public static CloudHeadLink Preconnect(string href, string? crossOrigin = null) => new()
    {
        Rel = "preconnect",
        Href = href,
        CrossOrigin = crossOrigin
    };
}

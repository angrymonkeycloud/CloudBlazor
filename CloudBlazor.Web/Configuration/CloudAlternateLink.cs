namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// A localized variant of the current page, rendered as
/// <c>&lt;link rel="alternate" hreflang="…" href="…" /&gt;</c>.
/// </summary>
/// <param name="HrefLang">
/// A language or language-region code (<c>en</c>, <c>en-GB</c>, <c>ar</c>), or
/// <see cref="XDefault"/> for the fallback shown to unmatched locales.
/// </param>
/// <param name="Href">Absolute URL of the variant.</param>
public readonly record struct CloudAlternateLink(string HrefLang, string Href)
{
    /// <summary>
    /// The <c>x-default</c> token, marking the page served when no other language matches.
    /// </summary>
    public const string XDefault = "x-default";
}

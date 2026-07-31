using System.Globalization;
using System.Text;
using System.Xml;

namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// Collects sitemap entries and renders them as sitemap-protocol XML.
/// </summary>
public class CloudSitemap
{
    private readonly List<CloudSitemapEntry> _entries = [];

    /// <summary>The entries added so far.</summary>
    public IReadOnlyList<CloudSitemapEntry> Entries => _entries;

    /// <summary>Adds a URL.</summary>
    /// <param name="location">Absolute URL, or a path resolved against the request origin.</param>
    /// <param name="lastModified">When the page last changed.</param>
    /// <param name="changeFrequency">How often the page is expected to change.</param>
    /// <param name="priority">Relative importance within this site, <c>0.0</c> to <c>1.0</c>.</param>
    /// <param name="alternates">Localized variants of this URL.</param>
    public CloudSitemap Add(
        string location,
        DateTimeOffset? lastModified = null,
        CloudChangeFrequencies? changeFrequency = null,
        double? priority = null,
        IReadOnlyList<CloudAlternateLink>? alternates = null) =>
        Add(new CloudSitemapEntry
        {
            Location = location,
            LastModified = lastModified,
            ChangeFrequency = changeFrequency,
            Priority = priority,
            Alternates = alternates ?? []
        });

    /// <summary>Adds a URL.</summary>
    public CloudSitemap Add(CloudSitemapEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _entries.Add(entry);

        return this;
    }

    /// <summary>Adds several URLs.</summary>
    public CloudSitemap AddRange(IEnumerable<CloudSitemapEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        _entries.AddRange(entries);

        return this;
    }

    /// <summary>
    /// Adds one URL per language for the same page, cross-linking every variant to all the
    /// others. Search engines require the alternate set to be complete and reciprocal, which
    /// is tedious and easy to get wrong by hand.
    /// </summary>
    /// <param name="alternates">Every language variant of the page.</param>
    /// <param name="xDefault">
    /// The <c>hreflang</c> whose URL is also advertised as <c>x-default</c>. Ignored when it
    /// matches no alternate.
    /// </param>
    /// <param name="lastModified">When the page last changed.</param>
    /// <param name="changeFrequency">How often the page is expected to change.</param>
    /// <param name="priority">Relative importance within this site.</param>
    public CloudSitemap AddLocalized(
        IReadOnlyList<CloudAlternateLink> alternates,
        string? xDefault = null,
        DateTimeOffset? lastModified = null,
        CloudChangeFrequencies? changeFrequency = null,
        double? priority = null)
    {
        ArgumentNullException.ThrowIfNull(alternates);

        if (alternates.Count == 0)
            return this;

        List<CloudAlternateLink> set = [.. alternates];

        if (!string.IsNullOrWhiteSpace(xDefault))
        {
            CloudAlternateLink match = alternates.FirstOrDefault(alternate =>
                string.Equals(alternate.HrefLang, xDefault, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(match.Href))
                set.Add(new CloudAlternateLink(CloudAlternateLink.XDefault, match.Href));
        }

        foreach (CloudAlternateLink alternate in alternates)
            Add(alternate.Href, lastModified, changeFrequency, priority, set);

        return this;
    }

    /// <summary>
    /// Renders the sitemap XML.
    /// </summary>
    /// <param name="baseUrl">
    /// Origin used to make relative locations absolute. The protocol requires absolute URLs.
    /// </param>
    public string ToXml(string? baseUrl = null)
    {
        using Utf8StringWriter builder = new();

        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = "\t"
        };

        using (XmlWriter writer = XmlWriter.Create(builder, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            bool hasAlternates = _entries.Any(entry => entry.Alternates.Count > 0);

            // Declared only when used: an unused namespace declaration on every sitemap is
            // noise, and some validators flag it.
            if (hasAlternates)
                writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");

            foreach (CloudSitemapEntry entry in _entries)
            {
                writer.WriteStartElement("url");

                writer.WriteElementString("loc", CloudPage.ToAbsolute(entry.Location, baseUrl) ?? entry.Location);

                if (entry.LastModified.HasValue)
                    writer.WriteElementString("lastmod", entry.LastModified.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

                if (entry.ChangeFrequency.HasValue)
                    writer.WriteElementString("changefreq", entry.ChangeFrequency.Value.ToString().ToLowerInvariant());

                if (entry.Priority.HasValue)
                    writer.WriteElementString("priority", entry.Priority.Value.ToString("0.0", CultureInfo.InvariantCulture));

                foreach (CloudAlternateLink alternate in entry.Alternates)
                {
                    writer.WriteStartElement("link", "http://www.w3.org/1999/xhtml");
                    writer.WriteAttributeString("rel", "alternate");
                    writer.WriteAttributeString("hreflang", alternate.HrefLang);
                    writer.WriteAttributeString("href", CloudPage.ToAbsolute(alternate.Href, baseUrl) ?? alternate.Href);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return builder.ToString();
    }

    /// <summary>
    /// A <see cref="StringWriter"/> that reports UTF-8.
    /// </summary>
    /// <remarks>
    /// <see cref="XmlWriter"/> takes the encoding for its declaration from the writer, not from
    /// <see cref="XmlWriterSettings.Encoding"/>, and a <see cref="StringWriter"/> reports UTF-16
    /// because that is how .NET holds a string. The document is then served as UTF-8, leaving
    /// <c>&lt;?xml version="1.0" encoding="utf-16"?&gt;</c> contradicting the response — which
    /// strict parsers reject outright.
    /// </remarks>
    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}

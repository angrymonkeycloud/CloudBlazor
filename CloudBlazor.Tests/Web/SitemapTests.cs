using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using System.Xml.Linq;
using Xunit;

namespace CloudBlazor.Tests.Web;

/// <summary>
/// Sitemap XML generation. Assertions parse the output rather than matching strings, so
/// formatting changes do not break them but a malformed document does.
/// </summary>
public class SitemapTests
{
    private static readonly XNamespace Sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace Xhtml = "http://www.w3.org/1999/xhtml";

    private static XDocument Parse(string xml) => XDocument.Parse(xml);

    [Fact]
    public void ToXml_DeclaresUtf8()
    {
        // The document is served as UTF-8. XmlWriter takes the declaration from the
        // underlying writer, and a plain StringWriter reports UTF-16, which contradicts the
        // response and is rejected by strict parsers.
        CloudSitemap sitemap = new();
        sitemap.Add("/");

        sitemap.ToXml("https://example.com").Should().StartWith("""<?xml version="1.0" encoding="utf-8"?>""");
    }

    [Fact]
    public void ToXml_ProducesAValidUrlSet()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("/");

        XDocument document = Parse(sitemap.ToXml("https://example.com"));

        document.Root!.Name.Should().Be(Sitemap + "urlset");
    }

    [Fact]
    public void ToXml_MakesRelativeLocationsAbsolute()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("/about");

        XDocument document = Parse(sitemap.ToXml("https://example.com"));

        document.Root!.Element(Sitemap + "url")!
            .Element(Sitemap + "loc")!.Value.Should().Be("https://example.com/about");
    }

    [Fact]
    public void ToXml_LeavesAbsoluteLocationsUnchanged()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("https://other.example.com/page");

        Parse(sitemap.ToXml("https://example.com")).Root!
            .Element(Sitemap + "url")!
            .Element(Sitemap + "loc")!.Value.Should().Be("https://other.example.com/page");
    }

    [Fact]
    public void ToXml_WritesEveryEntry()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("/").Add("/about").Add("/contact");

        Parse(sitemap.ToXml("https://example.com")).Root!
            .Elements(Sitemap + "url").Should().HaveCount(3);
    }

    [Fact]
    public void ToXml_WritesOptionalMetadata()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("/about",
            lastModified: new DateTimeOffset(2026, 3, 14, 10, 30, 0, TimeSpan.Zero),
            changeFrequency: CloudChangeFrequencies.Monthly,
            priority: 0.8);

        XElement url = Parse(sitemap.ToXml("https://example.com")).Root!.Element(Sitemap + "url")!;

        url.Element(Sitemap + "lastmod")!.Value.Should().Be("2026-03-14");
        url.Element(Sitemap + "changefreq")!.Value.Should().Be("monthly");
        url.Element(Sitemap + "priority")!.Value.Should().Be("0.8");
    }

    [Fact]
    public void ToXml_OmitsOptionalMetadata_WhenNotSet()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("/about");

        XElement url = Parse(sitemap.ToXml("https://example.com")).Root!.Element(Sitemap + "url")!;

        url.Element(Sitemap + "lastmod").Should().BeNull();
        url.Element(Sitemap + "changefreq").Should().BeNull();
        url.Element(Sitemap + "priority").Should().BeNull();
    }

    [Fact]
    public void ToXml_FormatsPriorityInvariantly()
    {
        // A comma decimal separator under a European culture would be invalid here.
        System.Globalization.CultureInfo original = System.Globalization.CultureInfo.CurrentCulture;

        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");

            CloudSitemap sitemap = new();
            sitemap.Add("/", priority: 0.5);

            sitemap.ToXml("https://example.com").Should().Contain("<priority>0.5</priority>");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void ToXml_OmitsXhtmlNamespace_WhenNoAlternatesExist()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("/");

        sitemap.ToXml("https://example.com").Should().NotContain("xhtml");
    }

    [Fact]
    public void ToXml_WritesAlternatesAsXhtmlLinks()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("/about", alternates:
        [
            new CloudAlternateLink("en", "/about"),
            new CloudAlternateLink("ar", "/ar/about")
        ]);

        XElement url = Parse(sitemap.ToXml("https://example.com")).Root!.Element(Sitemap + "url")!;

        List<XElement> links = [.. url.Elements(Xhtml + "link")];

        links.Should().HaveCount(2);
        links[0].Attribute("hreflang")!.Value.Should().Be("en");
        links[0].Attribute("href")!.Value.Should().Be("https://example.com/about");
        links[1].Attribute("hreflang")!.Value.Should().Be("ar");
    }

    // ── AddLocalized ──────────────────────────────────────────────────────

    [Fact]
    public void AddLocalized_WritesOneUrlPerLanguage()
    {
        CloudSitemap sitemap = new();
        sitemap.AddLocalized(
        [
            new CloudAlternateLink("en", "/about"),
            new CloudAlternateLink("ar", "/ar/about")
        ]);

        Parse(sitemap.ToXml("https://example.com")).Root!
            .Elements(Sitemap + "url").Should().HaveCount(2);
    }

    [Fact]
    public void AddLocalized_CrossLinksEveryVariantFromEachUrl()
    {
        // Search engines require the alternate set to be reciprocal and complete.
        CloudSitemap sitemap = new();
        sitemap.AddLocalized(
        [
            new CloudAlternateLink("en", "/about"),
            new CloudAlternateLink("ar", "/ar/about")
        ]);

        foreach (XElement url in Parse(sitemap.ToXml("https://example.com")).Root!.Elements(Sitemap + "url"))
            url.Elements(Xhtml + "link").Should().HaveCount(2);
    }

    [Fact]
    public void AddLocalized_AddsXDefaultForTheNominatedLanguage()
    {
        CloudSitemap sitemap = new();
        sitemap.AddLocalized(
        [
            new CloudAlternateLink("en", "/about"),
            new CloudAlternateLink("ar", "/ar/about")
        ], xDefault: "en");

        XElement url = Parse(sitemap.ToXml("https://example.com")).Root!.Element(Sitemap + "url")!;

        XElement xDefault = url.Elements(Xhtml + "link")
            .Single(link => link.Attribute("hreflang")!.Value == CloudAlternateLink.XDefault);

        xDefault.Attribute("href")!.Value.Should().Be("https://example.com/about");
    }

    [Fact]
    public void AddLocalized_IgnoresUnknownXDefaultLanguage()
    {
        CloudSitemap sitemap = new();
        sitemap.AddLocalized([new CloudAlternateLink("en", "/about")], xDefault: "fr");

        sitemap.ToXml("https://example.com").Should().NotContain(CloudAlternateLink.XDefault);
    }

    [Fact]
    public void AddLocalized_IgnoresAnEmptySet()
    {
        CloudSitemap sitemap = new();
        sitemap.AddLocalized([]);

        sitemap.Entries.Should().BeEmpty();
    }

    [Fact]
    public void Add_ReturnsThis_ForFluentChaining()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("/").Should().BeSameAs(sitemap);
    }

    [Fact]
    public void Add_ThrowsOnNullEntry()
    {
        CloudSitemap sitemap = new();
        Action act = () => sitemap.Add((CloudSitemapEntry)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToXml_HandlesAnEmptySitemap()
    {
        Parse(new CloudSitemap().ToXml("https://example.com")).Root!
            .Elements(Sitemap + "url").Should().BeEmpty();
    }

    [Fact]
    public void ToXml_EscapesUrlsContainingAmpersands()
    {
        CloudSitemap sitemap = new();
        sitemap.Add("https://example.com/search?a=1&b=2");

        string xml = sitemap.ToXml();

        xml.Should().Contain("&amp;");

        Parse(xml).Root!.Element(Sitemap + "url")!
            .Element(Sitemap + "loc")!.Value.Should().Be("https://example.com/search?a=1&b=2");
    }
}

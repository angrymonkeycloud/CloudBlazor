using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace CloudBlazor.Tests.Web;

/// <summary>
/// Canonical URLs, language alternates, Open Graph, Twitter cards and structured data.
/// </summary>
public class CloudPageSeoTests
{
    // ── Canonical ─────────────────────────────────────────────────────────

    [Fact]
    public void SetCanonical_StoresValue()
    {
        CloudPage page = new();
        page.SetCanonical("https://example.com/about");
        page.Canonical.Should().Be("https://example.com/about");
    }

    [Fact]
    public void SetCanonical_ReturnsThis_ForFluentChaining()
    {
        CloudPage page = new();
        page.SetCanonical("/about").Should().BeSameAs(page);
    }

    [Fact]
    public void SetCanonical_FiresOnModified()
    {
        CloudPage page = new();
        bool fired = false;
        page.OnModified += () => fired = true;
        page.SetCanonical("/about");
        fired.Should().BeTrue();
    }

    [Fact]
    public void CanonicalResult_MakesRelativeUrlAbsolute()
    {
        CloudPage page = new();
        page.SetCanonical("/about");
        page.CanonicalResult("https://example.com").Should().Be("https://example.com/about");
    }

    [Fact]
    public void CanonicalResult_LeavesAbsoluteUrlUnchanged()
    {
        CloudPage page = new();
        page.SetCanonical("https://cdn.example.com/about");
        page.CanonicalResult("https://example.com").Should().Be("https://cdn.example.com/about");
    }

    [Fact]
    public void CanonicalResult_ReturnsRelativeUrl_WhenNoBaseUrl()
    {
        // Dropping it would remove the canonical tag entirely; a relative canonical is
        // still valid and resolves against the document.
        CloudPage page = new();
        page.SetCanonical("/about");
        page.CanonicalResult().Should().Be("/about");
    }

    [Fact]
    public void CanonicalResult_ReturnsNull_WhenNotSet()
    {
        new CloudPage().CanonicalResult("https://example.com").Should().BeNull();
    }

    // ── Alternates ────────────────────────────────────────────────────────

    [Fact]
    public void AddAlternate_StoresLanguageAndUrl()
    {
        CloudPage page = new();
        page.AddAlternate("en", "https://example.com/about");

        page.Alternates.Should().ContainSingle()
            .Which.Should().Be(new CloudAlternateLink("en", "https://example.com/about"));
    }

    [Fact]
    public void AddAlternate_ReplacesDuplicateLanguage()
    {
        CloudPage page = new();
        page.AddAlternate("en", "https://example.com/old");
        page.AddAlternate("en", "https://example.com/new");

        page.Alternates.Should().ContainSingle()
            .Which.Href.Should().Be("https://example.com/new");
    }

    [Fact]
    public void AddAlternate_TreatsLanguageCaseInsensitively()
    {
        CloudPage page = new();
        page.AddAlternate("en", "https://example.com/a");
        page.AddAlternate("EN", "https://example.com/b");

        page.Alternates.Should().ContainSingle();
    }

    [Fact]
    public void AlternatesResult_MakesRelativeUrlsAbsolute()
    {
        CloudPage page = new();
        page.AddAlternate("ar", "/ar/about");

        page.AlternatesResult("https://example.com").Should().ContainSingle()
            .Which.Href.Should().Be("https://example.com/ar/about");
    }

    [Fact]
    public void SetAlternates_ReplacesEveryExistingEntry()
    {
        CloudPage page = new();
        page.AddAlternate("en", "/a");
        page.SetAlternates([new CloudAlternateLink("fr", "/fr")]);

        page.Alternates.Should().ContainSingle().Which.HrefLang.Should().Be("fr");
    }

    [Fact]
    public void XDefault_IsTheExpectedToken()
    {
        CloudAlternateLink.XDefault.Should().Be("x-default");
    }

    // ── Open Graph ────────────────────────────────────────────────────────

    [Fact]
    public void OpenGraphTypeResult_DefaultsToWebsite()
    {
        new CloudPage().OpenGraphTypeResult().Should().Be("website");
    }

    [Fact]
    public void OpenGraphTypeResult_UsesExplicitValue()
    {
        CloudPage page = new();
        page.SetOpenGraphType("article");
        page.OpenGraphTypeResult().Should().Be("article");
    }

    [Fact]
    public void SocialTitleResult_FallsBackToPageTitle()
    {
        CloudPage page = new();
        page.SetTitle("About");
        page.SocialTitleResult().Should().Be("About");
    }

    [Fact]
    public void SocialTitleResult_PrefersExplicitSocialTitle()
    {
        CloudPage page = new();
        page.SetTitle("About").SetSocialTitle("About our team");
        page.SocialTitleResult().Should().Be("About our team");
    }

    [Fact]
    public void SocialDescriptionResult_FallsBackToDescription()
    {
        CloudPage page = new();
        page.SetDescription("Meta description.");
        page.SocialDescriptionResult().Should().Be("Meta description.");
    }

    [Fact]
    public void SocialDescriptionResult_IsNotTruncatedAt160Chars()
    {
        // The 160-character limit is a search-snippet concern; link previews show more.
        string longText = new('a', 200);
        CloudPage page = new();
        page.SetDescription(longText);

        page.SocialDescriptionResult().Should().HaveLength(200);
    }

    [Fact]
    public void AddLocaleAlternates_IgnoresDuplicates()
    {
        CloudPage page = new();
        page.AddLocaleAlternates("ar_AR", "AR_ar");

        page.LocaleAlternates.Should().ContainSingle();
    }

    // ── Images ────────────────────────────────────────────────────────────

    [Fact]
    public void SetImage_FromUrl_StoresImage()
    {
        CloudPage page = new();
        page.SetImage("https://example.com/og.png");
        page.Image!.Url.Should().Be("https://example.com/og.png");
    }

    [Fact]
    public void ImageResult_MakesRelativeUrlAbsolute()
    {
        CloudPage page = new();
        page.SetImage("/img/og.png");
        page.ImageResult("https://example.com")!.Url.Should().Be("https://example.com/img/og.png");
    }

    [Fact]
    public void ImageResult_PreservesDimensionsAndAlt_WhenRewritingUrl()
    {
        CloudPage page = new();
        page.SetImage(new CloudPageImage { Url = "/og.png", Width = 1200, Height = 630, Alt = "Logo" });

        CloudPageImage result = page.ImageResult("https://example.com")!;

        result.Width.Should().Be(1200);
        result.Height.Should().Be(630);
        result.Alt.Should().Be("Logo");
    }

    [Fact]
    public void ImageResult_ReturnsNull_WhenNoImageSet()
    {
        new CloudPage().ImageResult("https://example.com").Should().BeNull();
    }

    [Theory]
    [InlineData("/og.png", "image/png")]
    [InlineData("/og.jpg", "image/jpeg")]
    [InlineData("/og.jpeg", "image/jpeg")]
    [InlineData("/og.webp", "image/webp")]
    [InlineData("/og.svg", "image/svg+xml")]
    [InlineData("/og.bin", null)]
    public void MimeTypeResult_IsInferredFromExtension(string url, string? expected)
    {
        new CloudPageImage { Url = url }.MimeTypeResult().Should().Be(expected);
    }

    [Fact]
    public void MimeTypeResult_IgnoresQueryString()
    {
        new CloudPageImage { Url = "/og.png?v=2" }.MimeTypeResult().Should().Be("image/png");
    }

    [Fact]
    public void MimeTypeResult_PrefersExplicitValue()
    {
        new CloudPageImage { Url = "/og.bin", MimeType = "image/png" }.MimeTypeResult().Should().Be("image/png");
    }

    // ── Twitter ───────────────────────────────────────────────────────────

    [Fact]
    public void TwitterCardResult_DefaultsToSummary_WithoutImage()
    {
        new CloudPage().TwitterCardResult().Should().Be(CloudTwitterCards.Summary);
    }

    [Fact]
    public void TwitterCardResult_DefaultsToLargeImage_WithImage()
    {
        CloudPage page = new();
        page.SetImage("/og.png");
        page.TwitterCardResult().Should().Be(CloudTwitterCards.SummaryLargeImage);
    }

    [Fact]
    public void TwitterCardResult_PrefersExplicitValue()
    {
        CloudPage page = new();
        page.SetImage("/og.png").SetTwitterCard(CloudTwitterCards.Summary);
        page.TwitterCardResult().Should().Be(CloudTwitterCards.Summary);
    }

    [Theory]
    [InlineData(CloudTwitterCards.Summary, "summary")]
    [InlineData(CloudTwitterCards.SummaryLargeImage, "summary_large_image")]
    [InlineData(CloudTwitterCards.App, "app")]
    [InlineData(CloudTwitterCards.Player, "player")]
    public void TwitterCardValueResult_UsesTheDocumentedTokens(CloudTwitterCards card, string expected)
    {
        CloudPage page = new();
        page.SetTwitterCard(card);
        page.TwitterCardValueResult().Should().Be(expected);
    }

    // ── Structured data ───────────────────────────────────────────────────

    [Fact]
    public void AddStructuredData_StoresRawJson()
    {
        CloudPage page = new();
        page.AddStructuredData("""{"@type":"Organization"}""");

        page.StructuredData.Should().ContainSingle()
            .Which.Should().Be("""{"@type":"Organization"}""");
    }

    [Fact]
    public void AddStructuredData_IgnoresEmptyJson()
    {
        CloudPage page = new();
        page.AddStructuredData("   ");
        page.StructuredData.Should().BeEmpty();
    }

    [Fact]
    public void AddStructuredData_SerializesObjects()
    {
        CloudPage page = new();
        page.AddStructuredData(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Organization",
            ["name"] = "Angry Monkey"
        });

        string json = page.StructuredData.Should().ContainSingle().Subject;

        using JsonDocument document = JsonDocument.Parse(json);

        document.RootElement.GetProperty("@type").GetString().Should().Be("Organization");
        document.RootElement.GetProperty("name").GetString().Should().Be("Angry Monkey");
    }

    [Fact]
    public void AddStructuredData_DoesNotEscapeNonAsciiText()
    {
        // Escaping would triple the size of Arabic or Chinese structured data for no gain.
        CloudPage page = new();
        page.AddStructuredData(new Dictionary<string, object> { ["name"] = "محلولة" });

        page.StructuredData.Single().Should().Contain("محلولة");
    }

    [Fact]
    public void AddStructuredData_OmitsNullProperties()
    {
        // Lets an optional schema.org property be left unset without emitting "logo": null.
        CloudPage page = new();
        page.AddStructuredData(new { name = "Set", logo = (string?)null });

        page.StructuredData.Single().Should().NotContain("logo");
    }

    [Fact]
    public void AddStructuredData_KeepsNullDictionaryEntries()
    {
        // WhenWritingNull governs object properties, not dictionary entries: a dictionary
        // is data, and dropping a key the caller explicitly supplied would be surprising.
        CloudPage page = new();
        page.AddStructuredData(new Dictionary<string, object?> { ["name"] = "Set", ["logo"] = null });

        page.StructuredData.Single().Should().Contain("logo");
    }

    [Fact]
    public void AddStructuredData_AccumulatesDocuments()
    {
        CloudPage page = new();
        page.AddStructuredData("""{"@type":"Organization"}""");
        page.AddStructuredData("""{"@type":"WebSite"}""");

        page.StructuredData.Should().HaveCount(2);
    }

    [Fact]
    public void AddStructuredData_ThrowsOnNullObject()
    {
        CloudPage page = new();
        Action act = () => page.AddStructuredData((object)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Structured data: script-element safety ────────────────────────────

    [Fact]
    public void StructuredDataResult_EscapesClosingScriptTag_FromSerializedObjects()
    {
        // Without this, a value carrying "</script>" closes the element and everything
        // after it is executed as markup — script injection on every consuming site.
        CloudPage page = new();
        page.AddStructuredData(new { name = "</script><script>alert(1)</script>" });

        page.StructuredDataResult().Single().Should().NotContain("</script>");
    }

    [Fact]
    public void StructuredDataResult_EscapesClosingScriptTag_FromRawJson()
    {
        // Raw JSON never passes through the serializer, so it is escaped at render instead.
        CloudPage page = new();
        page.AddStructuredData("""{"name":"</script><script>alert(1)</script>"}""");

        page.StructuredDataResult().Single().Should().NotContain("</script>");
    }

    [Fact]
    public void StructuredDataResult_StaysValidJson_AfterEscaping()
    {
        CloudPage page = new();
        page.AddStructuredData("""{"name":"a < b"}""");

        using JsonDocument document = JsonDocument.Parse(page.StructuredDataResult().Single());

        document.RootElement.GetProperty("name").GetString().Should().Be("a < b");
    }

    [Fact]
    public void StructuredDataResult_KeepsNonAsciiReadable()
    {
        CloudPage page = new();
        page.AddStructuredData(new Dictionary<string, object> { ["name"] = "محلولة" });

        page.StructuredDataResult().Single().Should().Contain("محلولة");
    }

    [Fact]
    public void ClearStructuredData_RemovesEveryDocument()
    {
        CloudPage page = new();
        page.AddStructuredData("""{"@type":"Organization"}""");
        page.ClearStructuredData();

        page.StructuredData.Should().BeEmpty();
    }
}

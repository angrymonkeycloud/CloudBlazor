using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using Xunit;

namespace CloudBlazor.Tests.Web;

/// <summary>
/// Mapping between a site's languages and its URLs.
/// </summary>
public class LocalizationOptionsTests
{
    private static CloudLocalizationOptions Options() => new()
    {
        DefaultCulture = "en",
        SupportedCultures = ["en", "ar"],
        Locales = new Dictionary<string, string> { ["en"] = "en_US", ["ar"] = "ar_AR" }
    };

    // ── Neutral path ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("/", "")]
    [InlineData("/about", "about")]
    [InlineData("/ar", "")]
    [InlineData("/ar/about", "about")]
    [InlineData("/ar/about/team", "about/team")]
    [InlineData("/en/about", "about")]
    public void NeutralPath_StripsTheCulturePrefix(string path, string expected)
    {
        Options().NeutralPath(path).Should().Be(expected);
    }

    [Fact]
    public void NeutralPath_KeepsSegmentsThatAreNotCultures()
    {
        // "articles" must not be mistaken for a language code.
        Options().NeutralPath("/articles/ar").Should().Be("articles/ar");
    }

    [Fact]
    public void NeutralPath_DropsQueryAndFragment()
    {
        Options().NeutralPath("/about?utm_source=x#top").Should().Be("about");
    }

    [Fact]
    public void NeutralPath_HandlesEmptyInput()
    {
        Options().NeutralPath(null).Should().BeEmpty();
    }

    // ── Culture of a path ─────────────────────────────────────────────────

    [Theory]
    [InlineData("/", "en")]
    [InlineData("/about", "en")]
    [InlineData("/ar", "ar")]
    [InlineData("/ar/about", "ar")]
    [InlineData("/fr/about", "en")]
    public void CultureOf_ReadsThePrefixOrFallsBackToDefault(string path, string expected)
    {
        Options().CultureOf(path).Should().Be(expected);
    }

    [Fact]
    public void CultureOf_NormalisesCasingToTheConfiguredValue()
    {
        Options().CultureOf("/AR/about").Should().Be("ar");
    }

    // ── Path generation ───────────────────────────────────────────────────

    [Theory]
    [InlineData("en", "", "/")]
    [InlineData("en", "about", "/about")]
    [InlineData("ar", "", "/ar")]
    [InlineData("ar", "about", "/ar/about")]
    public void PathFor_BuildsTheUrlForACulture(string culture, string neutral, string expected)
    {
        Options().PathFor(culture, neutral).Should().Be(expected);
    }

    [Fact]
    public void PathFor_ToleratesSurroundingSlashes()
    {
        Options().PathFor("ar", "/about/").Should().Be("/ar/about");
    }

    // ── Alternates ────────────────────────────────────────────────────────

    [Fact]
    public void AlternatesFor_ProducesEveryCulturePlusXDefault()
    {
        List<CloudAlternateLink> alternates = [.. Options().AlternatesFor("about")];

        alternates.Should().HaveCount(3);
        alternates.Should().Contain(new CloudAlternateLink("en", "/about"));
        alternates.Should().Contain(new CloudAlternateLink("ar", "/ar/about"));
        alternates.Should().Contain(new CloudAlternateLink(CloudAlternateLink.XDefault, "/about"));
    }

    [Fact]
    public void AlternatesFor_PointsXDefaultAtTheDefaultCulture()
    {
        CloudAlternateLink xDefault = Options().AlternatesFor("about")
            .Single(alternate => alternate.HrefLang == CloudAlternateLink.XDefault);

        xDefault.Href.Should().Be("/about");
    }

    [Fact]
    public void AlternatesFor_HandlesTheHomePage()
    {
        List<CloudAlternateLink> alternates = [.. Options().AlternatesFor("")];

        alternates.Should().Contain(new CloudAlternateLink("en", "/"));
        alternates.Should().Contain(new CloudAlternateLink("ar", "/ar"));
    }

    /// <summary>
    /// Every alternate set has to be reciprocal: the set generated from a page in one language
    /// must be identical to the set generated from its translation, or search engines discard
    /// the relationship.
    /// </summary>
    [Fact]
    public void AlternatesFor_IsIdenticalForEveryTranslationOfAPage()
    {
        CloudLocalizationOptions options = Options();

        string fromEnglish = string.Join('|', options.AlternatesFor(options.NeutralPath("/about")));
        string fromArabic = string.Join('|', options.AlternatesFor(options.NeutralPath("/ar/about")));

        fromArabic.Should().Be(fromEnglish);
    }

    // ── Locales ───────────────────────────────────────────────────────────

    [Fact]
    public void LocaleFor_UsesTheConfiguredMapping()
    {
        Options().LocaleFor("ar").Should().Be("ar_AR");
    }

    [Fact]
    public void LocaleFor_FallsBackToTheCultureName()
    {
        CloudLocalizationOptions options = new()
        {
            DefaultCulture = "en",
            SupportedCultures = ["en", "pt-BR"]
        };

        options.LocaleFor("pt-BR").Should().Be("pt_BR");
    }

    // ── Culture detection ─────────────────────────────────────────────────

    [Theory]
    [InlineData("en", true)]
    [InlineData("ar", true)]
    [InlineData("AR", true)]
    [InlineData("fr", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsCultureSegment_MatchesSupportedCulturesOnly(string? segment, bool expected)
    {
        Options().IsCultureSegment(segment).Should().Be(expected);
    }
}

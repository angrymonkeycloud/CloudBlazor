using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using Xunit;

namespace CloudBlazor.Tests.Web;

public class CloudPageTests
{
    // ── SetTitle ──────────────────────────────────────────────────────────

    [Fact]
    public void SetTitle_StoresTitle()
    {
        CloudPage page = new();
        page.SetTitle("Hello World");
        page.Title.Should().Be("Hello World");
    }

    [Fact]
    public void SetTitle_ReturnsThis_ForFluentChaining()
    {
        CloudPage page = new();
        CloudPage result = page.SetTitle("Test");
        result.Should().BeSameAs(page);
    }

    [Fact]
    public void SetTitle_FiresOnModified()
    {
        CloudPage page = new();
        bool fired = false;
        page.OnModified += () => fired = true;
        page.SetTitle("trigger");
        fired.Should().BeTrue();
    }

    // ── SetDescription ───────────────────────────────────────────────────

    [Fact]
    public void SetDescription_StoresDescription()
    {
        CloudPage page = new();
        page.SetDescription("A short description.");
        page.Description.Should().Be("A short description.");
    }

    [Fact]
    public void DescriptionResult_TruncatesAt160Chars()
    {
        string longDesc = new('a', 200);
        CloudPage page = new();
        page.SetDescription(longDesc);
        string? result = page.DescriptionResult();
        result.Should().HaveLength(160);
        result.Should().EndWith("...");
    }

    [Fact]
    public void DescriptionResult_DoesNotTruncateShortDescription()
    {
        string shortDesc = "Short.";
        CloudPage page = new();
        page.SetDescription(shortDesc);
        page.DescriptionResult().Should().Be(shortDesc);
    }

    [Fact]
    public void DescriptionResult_DoesNotCutTheFinalWord_WhenAUsefulBoundaryExists()
    {
        string description = string.Join(' ', Enumerable.Repeat("helpful", 30));
        CloudPage page = new();
        page.SetDescription(description);

        string result = page.DescriptionResult()!;

        result.Should().EndWith("helpful...");
        result.Should().NotContain("helpf...");
        result.Length.Should().BeLessThanOrEqualTo(160);
    }

    [Fact]
    public void DescriptionResult_ReturnsNull_WhenNotSet()
    {
        CloudPage page = new();
        page.DescriptionResult().Should().BeNull();
    }

    // ── SetKeywords ───────────────────────────────────────────────────────

    [Fact]
    public void SetKeywords_StoresKeywords()
    {
        CloudPage page = new();
        page.SetKeywords("blazor, seo");
        page.KeywordsResult().Should().Be("blazor, seo");
    }

    // ── SetFavicon ────────────────────────────────────────────────────────

    [Fact]
    public void SetFavicon_StoresFavicon()
    {
        CloudPage page = new();
        page.SetFavicon("/icons/favicon.png");
        page.FaviconResult().Should().Be("/icons/favicon.png");
    }

    [Theory]
    [InlineData("/favicon.ico", "image/x-icon")]
    [InlineData("/icon.svg?v=2", "image/svg+xml")]
    [InlineData("/icon.png#asset", "image/png")]
    public void FaviconTypeResult_InfersKnownImageTypes(string path, string expectedType)
    {
        CloudPage page = new();
        page.SetFavicon(path);
        page.FaviconTypeResult().Should().Be(expectedType);
    }

    [Fact]
    public void SetThemeColor_StoresThemeColor()
    {
        CloudPage page = new();
        page.SetThemeColor("#372549");
        page.ThemeColorResult().Should().Be("#372549");
    }

    [Fact]
    public void SetManifest_StoresManifest()
    {
        CloudPage page = new();
        page.SetManifest("/site.webmanifest");
        page.ManifestResult().Should().Be("/site.webmanifest");
    }

    [Fact]
    public void AddHeadLinks_StoresReusableHeadResources_InOrder()
    {
        CloudPage page = new();

        page.AddHeadLinks(
            AngryMonkey.CloudBlazor.CloudHeadLink.Icon("/favicon.svg", "image/svg+xml"),
            AngryMonkey.CloudBlazor.CloudHeadLink.Preload("/fonts/site.woff2", "font", "font/woff2", "anonymous"));

        page.HeadLinks.Select(link => link.Rel).Should().Equal("icon", "preload");
        page.HeadLinks[1].CrossOrigin.Should().Be("anonymous");
    }

    [Fact]
    public void AddHeadLink_ReplacesDuplicateRelAndHref()
    {
        CloudPage page = new();

        page.AddHeadLink(AngryMonkey.CloudBlazor.CloudHeadLink.Icon("/favicon.png", "image/png", "32x32"));
        page.AddHeadLink(AngryMonkey.CloudBlazor.CloudHeadLink.Icon("/favicon.png", "image/png", "96x96"));

        page.HeadLinks.Should().ContainSingle().Which.Sizes.Should().Be("96x96");
    }

    [Theory]
    [InlineData("/favicon.svg", "image/svg+xml")]
    [InlineData("/favicon.png?v=4", "image/png")]
    [InlineData("/fonts/site.woff2", "font/woff2")]
    [InlineData("/assets/site.css", "text/css")]
    public void HeadLink_TypeResult_InfersMimeTypeFromUrl(string href, string expected)
    {
        new AngryMonkey.CloudBlazor.CloudHeadLink { Rel = "preload", Href = href }
            .TypeResult().Should().Be(expected);
    }

    [Fact]
    public void HeadLink_TypeResult_PrefersExplicitType()
    {
        AngryMonkey.CloudBlazor.CloudHeadLink.Icon("/favicon.bin", "image/png")
            .TypeResult().Should().Be("image/png");
    }

    [Fact]
    public void LocalizedFontPreload_ResolvesBothHrefAndMimeType()
    {
        AngryMonkey.CloudBlazor.CloudHeadLink link = AngryMonkey.CloudBlazor.CloudHeadLink.LocalizedFontPreload(
            "/fonts/site.woff2",
            new Dictionary<string, string> { ["ar"] = "/fonts/site-ar.woff2" });

        link.HrefResult(new System.Globalization.CultureInfo("ar-LB")).Should().Be("/fonts/site-ar.woff2");
        link.TypeResult(new System.Globalization.CultureInfo("ar-LB")).Should().Be("font/woff2");
        link.CrossOrigin.Should().Be("anonymous");
    }

    [Fact]
    public void Reset_ClearsRouteSpecificMetadataAndCollections()
    {
        CloudPage page = new();
        page.SetTitle("Old route")
            .SetDescription("Old description")
            .SetCanonical("/old")
            .SetIndexPage(false)
            .AddHeadLink(AngryMonkey.CloudBlazor.CloudHeadLink.Icon("/old.svg"))
            .AddStructuredData(new { name = "Old route" });

        page.Reset();

        page.Title.Should().BeNull();
        page.Description.Should().BeNull();
        page.Canonical.Should().BeNull();
        page.IndexPage.Should().BeNull();
        page.HeadLinks.Should().BeEmpty();
        page.StructuredData.Should().BeEmpty();
    }

    // ── SetIndexPage / SetFollowPage / RobotsResult ───────────────────────

    [Fact]
    public void RobotsResult_ReturnsNull_WhenBothAllowed()
    {
        CloudPage page = new();
        page.SetIndexPage(true).SetFollowPage(true);
        page.RobotsResult().Should().BeNull();
    }

    [Fact]
    public void RobotsResult_ReturnsNoindex_WhenIndexFalse()
    {
        CloudPage page = new();
        page.SetIndexPage(false);
        page.RobotsResult().Should().Be("noindex");
    }

    [Fact]
    public void RobotsResult_ReturnsNofollow_WhenFollowFalse()
    {
        CloudPage page = new();
        page.SetFollowPage(false);
        page.RobotsResult().Should().Be("nofollow");
    }

    [Fact]
    public void RobotsResult_ReturnsBoth_WhenBothFalse()
    {
        CloudPage page = new();
        page.SetIndexPage(false).SetFollowPage(false);
        page.RobotsResult().Should().Be("noindex, nofollow");
    }

    [Fact]
    public void RobotsResult_ReturnsNull_WhenNeitherSet()
    {
        CloudPage page = new();
        page.RobotsResult().Should().BeNull();
    }

    // ── TitleAddOns ───────────────────────────────────────────────────────

    [Fact]
    public void SetTitleAddOns_ReplacesExistingAddOns()
    {
        CloudPage page = new();
        page.SetTitleAddOns(["#first"]);
        page.SetTitleAddOns(["#second", "#third"]);
        page.TitleAddOns.Should().BeEquivalentTo(["#second", "#third"]);
    }

    [Fact]
    public void SetTitleAddOns_FiresOnModified()
    {
        CloudPage page = new();
        bool fired = false;
        page.OnModified += () => fired = true;
        page.SetTitleAddOns(["#tag"]);
        fired.Should().BeTrue();
    }

    // ── TitleResult ───────────────────────────────────────────────────────

    [Fact]
    public void TitleResult_UsesPageTitle_WithPrefixAndSuffix()
    {
        CloudPage page = new();
        page.SetTitle("Dashboard");

        CloudWebConfig config = new()
        {
            TitlePrefix = "App | ",
            TitleSuffix = " — Docs"
        };

        page.TitleResult(config).Should().Be("App | Dashboard — Docs");
    }

    [Fact]
    public void TitleResult_FallsBackToDefaultTitle_WhenPageTitleEmpty()
    {
        CloudPage page = new();

        CloudWebConfig config = new();
        config.PageDefaults.SetTitle("Default Home");

        page.TitleResult(config).Should().Be("Default Home");
    }

    [Fact]
    public void TitleResult_AppendsTitleAddOns_WithinSixtyFourCharLimit()
    {
        CloudPage page = new();
        page.SetTitle("Home");
        page.SetTitleAddOns(["#tag1", "#tag2"]);

        CloudWebConfig config = new() { TitlePrefix = string.Empty, TitleSuffix = string.Empty };

        string? result = page.TitleResult(config);
        result.Should().NotBeNull();
        result!.Length.Should().BeLessThanOrEqualTo(64);
        result.Should().Contain("#tag1");
    }

    [Fact]
    public void TitleResult_DoesNotMutateDefaultTitleAddOns()
    {
        CloudWebConfig config = new();
        config.PageDefaults.SetTitle("Home");
        config.PageDefaults.SetTitleAddOns(["Default"]);

        CloudPage page = new();
        page.SetTitleAddOns(["Page"]);

        page.TitleResult(config);
        page.TitleResult(config);

        config.PageDefaults.TitleAddOns.Should().Equal("Default");
    }

    // ── AddLegacyExportsCreation ──────────────────────────────────────────

    [Fact]
    public void SetAddLegacyExportsCreation_StoresValue()
    {
        CloudPage page = new();
        page.SetAddLegacyExportsCreation(true);
        page.AddLegacyExportsCreation.Should().BeTrue();
    }
}

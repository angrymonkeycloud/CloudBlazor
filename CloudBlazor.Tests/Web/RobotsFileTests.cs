using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using Xunit;

namespace CloudBlazor.Tests.Web;

/// <summary>
/// <c>robots.txt</c> generation.
/// </summary>
public class RobotsFileTests
{
    private static string[] Lines(string content) =>
        [.. content.Split('\n').Select(line => line.TrimEnd('\r')).Where(line => line.Length > 0)];

    [Fact]
    public void ToFileContent_AllowsEverything_ByDefault()
    {
        Lines(new CloudRobotsFile().ToFileContent())
            .Should().Equal("User-agent: *", "Allow: /");
    }

    [Fact]
    public void ToFileContent_WritesAllowAndDisallowRules()
    {
        CloudRobotsFile robots = new();
        robots.Allow("/").Disallow("/admin");

        Lines(robots.ToFileContent())
            .Should().Equal("User-agent: *", "Allow: /", "Disallow: /admin");
    }

    [Fact]
    public void ToFileContent_GroupsRulesByUserAgent()
    {
        CloudRobotsFile robots = new();
        robots.Allow("/").Disallow("/private", "Googlebot");

        string content = robots.ToFileContent();

        content.Should().Contain("User-agent: *");
        content.Should().Contain("User-agent: Googlebot");
        content.Should().Contain("Disallow: /private");
    }

    [Fact]
    public void ToFileContent_ReusesAnExistingGroupForTheSameUserAgent()
    {
        CloudRobotsFile robots = new();
        robots.Disallow("/a", "Googlebot").Disallow("/b", "Googlebot");

        robots.Groups.Should().ContainSingle();
        robots.Groups[0].Disallowed.Should().Equal("/a", "/b");
    }

    [Fact]
    public void ToFileContent_MatchesUserAgentCaseInsensitively()
    {
        CloudRobotsFile robots = new();
        robots.Disallow("/a", "Googlebot").Disallow("/b", "googlebot");

        robots.Groups.Should().ContainSingle();
    }

    [Fact]
    public void ToFileContent_WritesCrawlDelay()
    {
        CloudRobotsFile robots = new();
        robots.Allow("/").CrawlDelay(1.5);

        robots.ToFileContent().Should().Contain("Crawl-delay: 1.5");
    }

    [Fact]
    public void ToFileContent_WritesCrawlDelayInvariantly()
    {
        System.Globalization.CultureInfo original = System.Globalization.CultureInfo.CurrentCulture;

        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");

            CloudRobotsFile robots = new();
            robots.CrawlDelay(1.5);

            robots.ToFileContent().Should().Contain("Crawl-delay: 1.5");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void ToFileContent_WritesSitemapUrls()
    {
        CloudRobotsFile robots = new();
        robots.Allow("/").AddSitemap("https://example.com/sitemap.xml");

        robots.ToFileContent().Should().Contain("Sitemap: https://example.com/sitemap.xml");
    }

    [Fact]
    public void ToFileContent_MakesRelativeSitemapUrlsAbsolute()
    {
        CloudRobotsFile robots = new();
        robots.AddSitemap("/sitemap.xml");

        robots.ToFileContent("https://example.com")
            .Should().Contain("Sitemap: https://example.com/sitemap.xml");
    }

    [Fact]
    public void AddSitemap_IgnoresDuplicates()
    {
        CloudRobotsFile robots = new();
        robots.AddSitemap("/sitemap.xml").AddSitemap("/sitemap.xml");

        robots.Sitemaps.Should().ContainSingle();
    }

    [Fact]
    public void AddSitemap_IgnoresEmptyValues()
    {
        CloudRobotsFile robots = new();
        robots.AddSitemap("  ");

        robots.Sitemaps.Should().BeEmpty();
    }

    [Fact]
    public void DisallowAll_BlocksTheWholeSite()
    {
        CloudRobotsFile robots = new();
        robots.DisallowAll();

        Lines(robots.ToFileContent()).Should().Equal("User-agent: *", "Disallow: /");
    }

    [Fact]
    public void DisallowAll_ReplacesPreviouslyConfiguredRules()
    {
        // The staging path calls this over an already-configured file; leaving an Allow
        // behind would let the crawl through.
        CloudRobotsFile robots = new();
        robots.Allow("/").Disallow("/admin");
        robots.DisallowAll();

        Lines(robots.ToFileContent()).Should().Equal("User-agent: *", "Disallow: /");
    }

    [Fact]
    public void Methods_ReturnThis_ForFluentChaining()
    {
        CloudRobotsFile robots = new();

        robots.Allow("/").Should().BeSameAs(robots);
        robots.Disallow("/a").Should().BeSameAs(robots);
        robots.CrawlDelay(1).Should().BeSameAs(robots);
        robots.AddSitemap("/sitemap.xml").Should().BeSameAs(robots);
        robots.DisallowAll().Should().BeSameAs(robots);
    }
}

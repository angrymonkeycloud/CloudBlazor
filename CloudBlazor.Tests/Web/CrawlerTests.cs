using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using Xunit;

namespace CloudBlazor.Tests.Web;

/// <summary>
/// Crawler detection and the non-production host check.
/// </summary>
public class CrawlerTests
{
    // ── The user-agent list ───────────────────────────────────────────────

    [Fact]
    public void CrawlersUserAgents_IsNotEmpty()
    {
        CloudWebConfig.CrawlersUserAgents.Should().NotBeEmpty();
    }

    [Fact]
    public void CrawlersUserAgents_ContainsCommonBots()
    {
        string[] expectedSubstrings = ["bot", "crawler", "spider", "baidu", "wget"];

        foreach (string agent in expectedSubstrings)
            CloudWebConfig.CrawlersUserAgents.Should().Contain(agent,
                because: $"'{agent}' is a well-known crawler user-agent substring");
    }

    [Fact]
    public void CrawlersUserAgents_AreAllLowerCase()
    {
        // Matching lower-cases the incoming user agent, so an entry containing any
        // upper-case character could never match. Normalizing the list is what makes the
        // bulk of these entries do anything at all.
        CloudWebConfig.CrawlersUserAgents.Should().OnlyContain(agent => agent == agent.ToLowerInvariant());
    }

    [Fact]
    public void CrawlersUserAgents_ContainsNoDuplicates()
    {
        CloudWebConfig.CrawlersUserAgents.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void CrawlersUserAgents_ContainsNoEmptyEntries()
    {
        CloudWebConfig.CrawlersUserAgents.Should().OnlyContain(agent => agent.Length > 0);
    }

    // ── IsCrawler ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Googlebot/2.1 (+http://www.google.com/bot.html)")]
    [InlineData("Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)")]
    [InlineData("Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)")]
    [InlineData("Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)")]
    [InlineData("Mozilla/5.0 (compatible; AhrefsBot/7.0; +http://ahrefs.com/robot/)")]
    [InlineData("curl/8.4.0")]
    [InlineData("Wget/1.21.3")]
    public void IsCrawler_MatchesKnownCrawlers(string userAgent)
    {
        CloudWebConfig.IsCrawler(userAgent).Should().BeTrue(because: $"'{userAgent}' is a known crawler");
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124.0 Safari/537.36")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 Safari/605.1.15")]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 Version/17.0 Mobile Safari/604.1")]
    public void IsCrawler_DoesNotMatchRealBrowsers(string userAgent)
    {
        CloudWebConfig.IsCrawler(userAgent).Should().BeFalse(because: $"'{userAgent}' is a real browser");
    }

    [Fact]
    public void IsCrawler_IsCaseInsensitive()
    {
        CloudWebConfig.IsCrawler("GOOGLEBOT/2.1").Should().BeTrue();
    }

    [Fact]
    public void IsCrawler_IgnoresSurroundingWhitespace()
    {
        CloudWebConfig.IsCrawler("   Googlebot/2.1   ").Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsCrawler_IsFalseForAMissingUserAgent(string? userAgent)
    {
        CloudWebConfig.IsCrawler(userAgent).Should().BeFalse();
    }

    [Fact]
    public void IsCrawler_FalseByDefault_WhenUsingParameterlessConstructor()
    {
        CloudPage page = new();

        page.IsCrawler.Should().BeFalse();
    }

    // ── IsNonProductionHost ───────────────────────────────────────────────

    [Theory]
    [InlineData("my-app.azurewebsites.net")]
    [InlineData("MY-APP.AZUREWEBSITES.NET")]
    [InlineData("staging.my-app.azurewebsites.net")]
    public void IsNonProductionHost_MatchesKnownPreviewSuffixes(string host)
    {
        CloudWebConfig.IsNonProductionHost(host).Should().BeTrue(
            because: "preview deployments must never be indexed");
    }

    [Theory]
    [InlineData("angrymonkeycloud.com")]
    [InlineData("www.angrymonkeycloud.com")]
    [InlineData("localhost")]
    public void IsNonProductionHost_DoesNotMatchProductionHosts(string host)
    {
        CloudWebConfig.IsNonProductionHost(host).Should().BeFalse();
    }

    [Fact]
    public void IsNonProductionHost_DoesNotMatchASuffixInTheMiddle()
    {
        CloudWebConfig.IsNonProductionHost("azurewebsites.net.example.com").Should().BeFalse(
            because: "only a host that ends with the suffix is a preview deployment");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsNonProductionHost_IsFalseForAMissingHost(string? host)
    {
        CloudWebConfig.IsNonProductionHost(host).Should().BeFalse();
    }
}

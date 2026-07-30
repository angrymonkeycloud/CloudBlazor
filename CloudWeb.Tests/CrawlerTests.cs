using AngryMonkey.CloudWeb;
using FluentAssertions;
using Xunit;

namespace CloudWeb.Tests;

/// <summary>
/// Tests for the crawler user-agent detection list and the static CrawlersUserAgents property.
/// Note: IsCrawler detection via IHttpContextAccessor is tested via the CloudPage(accessor) constructor
/// but that path requires mocking HttpContext — these tests cover the static list itself.
/// </summary>
public class CrawlerTests
{
    [Fact]
    public void CrawlersUserAgents_IsNotEmpty()
    {
        CloudWebConfig.CrawlersUserAgents.Should().NotBeEmpty();
    }

    [Fact]
    public void CrawlersUserAgents_ContainsCommonBots()
    {
        // The list stores substrings that are matched against user-agent strings.
        // "bot", "crawler", "spider" are present as standalone entries and match all major bots.
        string[] expectedSubstrings = ["bot", "crawler", "spider", "baidu", "wget"];
        foreach (string agent in expectedSubstrings)
            CloudWebConfig.CrawlersUserAgents.Should().Contain(agent,
                because: $"'{agent}' is a well-known crawler user-agent substring");
    }

    [Fact]
    public void IsCrawler_FalseByDefault_WhenUsingParameterlessConstructor()
    {
        CloudPage page = new();
        page.IsCrawler.Should().BeFalse();
    }

    [Theory]
    [InlineData("Googlebot/2.1 (+http://www.google.com/bot.html)")]
    [InlineData("Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)")]
    [InlineData("Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)")]
    public void CrawlerUserAgent_MatchesKnownPatterns(string userAgent)
    {
        string lower = userAgent.ToLower();
        bool isCrawler = CloudWebConfig.CrawlersUserAgents.Any(lower.Contains);
        isCrawler.Should().BeTrue(because: $"'{userAgent}' is a known crawler");
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124.0 Safari/537.36")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 Safari/605.1.15")]
    public void HumanUserAgent_DoesNotMatchCrawlerPatterns(string userAgent)
    {
        string lower = userAgent.ToLower();
        bool isCrawler = CloudWebConfig.CrawlersUserAgents.Any(lower.Contains);
        isCrawler.Should().BeFalse(because: $"'{userAgent}' is a real browser user-agent");
    }
}

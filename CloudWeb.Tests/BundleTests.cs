using AngryMonkey.CloudWeb;
using FluentAssertions;
using Xunit;

namespace CloudWeb.Tests;

public class BundleTests
{
    // ── AppendBundle (string) ─────────────────────────────────────────────

    [Fact]
    public void AppendBundle_String_AddsBundleWithSource()
    {
        CloudPage page = new();
        page.AppendBundle("~/js/app.js");
        page.Bundles.Should().ContainSingle(b => b.Source == "~/js/app.js");
    }

    [Fact]
    public void AppendBundle_String_FiresOnModified()
    {
        CloudPage page = new();
        bool fired = false;
        page.OnModified += () => fired = true;
        page.AppendBundle("~/js/app.js");
        fired.Should().BeTrue();
    }

    // ── AppendBundle (CloudBundle) ────────────────────────────────────────

    [Fact]
    public void AppendBundle_CloudBundle_AddsBundle()
    {
        CloudPage page = new();
        CloudBundle bundle = new() { Source = "~/css/theme.css", MinOnRelease = false, Defer = false };
        page.AppendBundle(bundle);
        page.Bundles.Should().ContainSingle(b => b.Source == "~/css/theme.css");
    }

    // ── AppendBundles (multiple strings) ─────────────────────────────────

    [Fact]
    public void AppendBundles_MultiplePaths_AddsAll()
    {
        CloudPage page = new();
        page.AppendBundles("~/css/a.css", "~/js/b.js", "~/js/c.js");
        page.Bundles.Should().HaveCount(3);
    }

    [Fact]
    public void AppendBundles_NullArgument_DoesNotThrow()
    {
        CloudPage page = new();
        page.Invoking(p => p.AppendBundles((string[]?)null))
            .Should().NotThrow();
    }

    [Fact]
    public void AppendBundles_NullCloudBundle_DoesNotThrow()
    {
        CloudPage page = new();
        page.Invoking(p => p.AppendBundles((CloudBundle[]?)null))
            .Should().NotThrow();
    }

    // ── InsertBundle ──────────────────────────────────────────────────────

    [Fact]
    public void InsertBundle_InsertsAtCorrectIndex()
    {
        CloudPage page = new();
        page.AppendBundle("~/js/second.js");
        page.AppendBundle("~/js/third.js");

        CloudBundle first = new() { Source = "~/js/first.js" };
        page.InsertBundle(0, first);

        page.Bundles[0].Source.Should().Be("~/js/first.js");
        page.Bundles[1].Source.Should().Be("~/js/second.js");
        page.Bundles[2].Source.Should().Be("~/js/third.js");
    }

    [Fact]
    public void InsertBundle_FiresOnModified()
    {
        CloudPage page = new();
        bool fired = false;
        page.OnModified += () => fired = true;
        page.InsertBundle(0, new CloudBundle { Source = "~/js/app.js" });
        fired.Should().BeTrue();
    }

    // ── Order preservation ────────────────────────────────────────────────

    [Fact]
    public void Bundles_PreserveInsertionOrder()
    {
        CloudPage page = new();
        page.AppendBundles("~/css/a.css", "~/js/b.js", "~/js/c.js");

        page.Bundles[0].Source.Should().Be("~/css/a.css");
        page.Bundles[1].Source.Should().Be("~/js/b.js");
        page.Bundles[2].Source.Should().Be("~/js/c.js");
    }

    // ── Fluent chaining ───────────────────────────────────────────────────

    [Fact]
    public void AppendBundle_ReturnsThis_ForFluentChaining()
    {
        CloudPage page = new();
        CloudPage result = page.AppendBundle("~/js/app.js");
        result.Should().BeSameAs(page);
    }

    [Fact]
    public void InsertBundle_ReturnsThis_ForFluentChaining()
    {
        CloudPage page = new();
        CloudPage result = page.InsertBundle(0, new CloudBundle { Source = "~/js/app.js" });
        result.Should().BeSameAs(page);
    }
}

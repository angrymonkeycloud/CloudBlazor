using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using Xunit;

namespace CloudBlazor.Tests.Web;

/// <summary>
/// The robots meta tag, including the preview and snippet directives.
/// </summary>
public class RobotsDirectiveTests
{
    [Fact]
    public void RobotsResult_ReturnsNull_WhenEverythingIsAllowed()
    {
        // No tag already means index and follow; emitting one would be noise.
        new CloudPage().RobotsResult().Should().BeNull();
    }

    [Fact]
    public void RobotsResult_EmitsNoIndex()
    {
        CloudPage page = new();
        page.SetIndexPage(false);
        page.RobotsResult().Should().Be("noindex");
    }

    [Fact]
    public void RobotsResult_EmitsNoIndexAndNoFollow()
    {
        CloudPage page = new();
        page.SetIndexPage(false).SetFollowPage(false);
        page.RobotsResult().Should().Be("noindex, nofollow");
    }

    [Fact]
    public void RobotsResult_EmitsMaxImagePreview()
    {
        CloudPage page = new();
        page.SetMaxImagePreview(CloudMaxImagePreviews.Large);
        page.RobotsResult().Should().Be("max-image-preview:large");
    }

    [Theory]
    [InlineData(CloudMaxImagePreviews.None, "max-image-preview:none")]
    [InlineData(CloudMaxImagePreviews.Standard, "max-image-preview:standard")]
    [InlineData(CloudMaxImagePreviews.Large, "max-image-preview:large")]
    public void RobotsResult_UsesTheDocumentedPreviewTokens(CloudMaxImagePreviews preview, string expected)
    {
        CloudPage page = new();
        page.SetMaxImagePreview(preview);
        page.RobotsResult().Should().Be(expected);
    }

    [Fact]
    public void RobotsResult_EmitsMaxSnippetAndVideoPreview()
    {
        CloudPage page = new();
        page.SetMaxSnippet(-1).SetMaxVideoPreview(-1);
        page.RobotsResult().Should().Be("max-snippet:-1, max-video-preview:-1");
    }

    [Fact]
    public void RobotsResult_EmitsZeroSnippet()
    {
        // 0 suppresses snippets and must not be confused with "unset".
        CloudPage page = new();
        page.SetMaxSnippet(0);
        page.RobotsResult().Should().Be("max-snippet:0");
    }

    [Fact]
    public void RobotsResult_EmitsNoArchive()
    {
        CloudPage page = new();
        page.SetNoArchive(true);
        page.RobotsResult().Should().Be("noarchive");
    }

    [Fact]
    public void RobotsResult_OmitsNoArchive_WhenFalse()
    {
        CloudPage page = new();
        page.SetNoArchive(false);
        page.RobotsResult().Should().BeNull();
    }

    [Fact]
    public void RobotsResult_CombinesEveryIndexableDirective()
    {
        CloudPage page = new();
        page.SetFollowPage(false)
            .SetNoArchive(true)
            .SetMaxImagePreview(CloudMaxImagePreviews.Large)
            .SetMaxSnippet(-1);

        page.RobotsResult().Should().Be("nofollow, noarchive, max-image-preview:large, max-snippet:-1");
    }

    [Fact]
    public void RobotsResult_DropsPreviewDirectives_WhenNoIndex()
    {
        // "noindex, max-image-preview:large" is self-contradictory: an excluded page has
        // no preview to size.
        CloudPage page = new();
        page.SetIndexPage(false)
            .SetMaxImagePreview(CloudMaxImagePreviews.Large)
            .SetMaxSnippet(-1)
            .SetNoArchive(true);

        page.RobotsResult().Should().Be("noindex");
    }

    [Fact]
    public void SetMaxImagePreview_FiresOnModified()
    {
        CloudPage page = new();
        bool fired = false;
        page.OnModified += () => fired = true;
        page.SetMaxImagePreview(CloudMaxImagePreviews.Large);
        fired.Should().BeTrue();
    }

    [Fact]
    public void RobotsSetters_ReturnThis_ForFluentChaining()
    {
        CloudPage page = new();

        page.SetNoArchive(true).Should().BeSameAs(page);
        page.SetMaxImagePreview(CloudMaxImagePreviews.Large).Should().BeSameAs(page);
        page.SetMaxSnippet(-1).Should().BeSameAs(page);
        page.SetMaxVideoPreview(-1).Should().BeSameAs(page);
    }
}

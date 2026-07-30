using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using Xunit;

namespace CloudBlazor.Tests.Web;

public class FeaturesTests
{
    [Fact]
    public void AddFeature_AddsFeatureToList()
    {
        CloudPage page = new();
        page.AddFeature(CloudPageFeatures.JQuery);
        page.Features.Should().Contain(CloudPageFeatures.JQuery);
    }

    [Fact]
    public void AddFeature_FiresOnModified()
    {
        CloudPage page = new();
        bool fired = false;
        page.OnModified += () => fired = true;
        page.AddFeature(CloudPageFeatures.JQuery);
        fired.Should().BeTrue();
    }

    [Fact]
    public void Features_StartsEmpty()
    {
        CloudPage page = new();
        page.Features.Should().BeEmpty();
    }
}

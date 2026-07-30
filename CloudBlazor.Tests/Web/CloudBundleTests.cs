using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using Xunit;

namespace CloudBlazor.Tests.Web;

/// <summary>
/// The bundle model's defaults, which are part of the public contract: changing one
/// silently changes how every consuming application emits its tags.
/// </summary>
public class CloudBundleTests
{
    [Fact]
    public void Defaults_MatchTheDocumentedBehaviour()
    {
        CloudBundle bundle = new() { Source = "css/site.css" };

        bundle.MinOnRelease.Should().BeTrue();
        bundle.AppendVersion.Should().BeTrue();
        bundle.UseMapping.Should().BeTrue();
        bundle.Defer.Should().BeTrue();
        bundle.Async.Should().BeFalse();
        bundle.AddOns.Should().BeNull();
    }

    [Fact]
    public void IsAPlainModel_NotAComponent()
    {
        // CloudBundle used to be both the model and the component that rendered it,
        // which raised BL0005 in every application that configured a bundle from code.
        // CloudBundleTag renders it now, and this asserts the split stays in place.
        typeof(CloudBundle).Should().NotBeAssignableTo<Microsoft.AspNetCore.Components.IComponent>();
    }

    [Fact]
    public void Properties_HaveNoComponentParameterAttribute()
    {
        IEnumerable<string> parameterProperties = typeof(CloudBundle)
            .GetProperties()
            .Where(property => property.GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.ParameterAttribute), inherit: true).Length > 0)
            .Select(property => property.Name);

        parameterProperties.Should().BeEmpty(
            because: "assigning a component parameter from application code is what BL0005 warns about");
    }
}

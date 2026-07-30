using AngryMonkey.CloudBlazor.App;
using AngryMonkey.CloudBlazor.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CloudBlazor.Tests.Web;

/// <summary>
/// The registration extensions, which are the entry point every application uses.
/// </summary>
public class ServiceRegistrationTests
{
    // ── AddCloudWeb ───────────────────────────────────────────────────────

    [Fact]
    public void AddCloudWeb_RegistersCloudPagePerRequest()
    {
        using ServiceProvider provider = new ServiceCollection().AddCloudWeb().BuildServiceProvider();

        using IServiceScope firstScope = provider.CreateScope();
        using IServiceScope secondScope = provider.CreateScope();

        CloudPage first = firstScope.ServiceProvider.GetRequiredService<CloudPage>();
        CloudPage second = secondScope.ServiceProvider.GetRequiredService<CloudPage>();

        first.Should().NotBeSameAs(second, because: "page metadata is per request");
        firstScope.ServiceProvider.GetRequiredService<CloudPage>().Should().BeSameAs(first);
    }

    [Fact]
    public void AddCloudWeb_RegistersTheHttpContextAccessor()
    {
        using ServiceProvider provider = new ServiceCollection().AddCloudWeb().BuildServiceProvider();

        provider.GetService<IHttpContextAccessor>().Should().NotBeNull(
            because: "CloudPage resolves crawler and host state from the request");
    }

    [Fact]
    public void AddCloudWeb_AppliesTheSuppliedDefaults()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddCloudWeb(config =>
            {
                config.TitleSuffix = " · CloudBlazor";
                config.PageDefaults.SetDescription("Default description.");
            })
            .BuildServiceProvider();

        CloudWebConfig config = provider.GetRequiredService<IOptions<CloudWebConfig>>().Value;

        config.TitleSuffix.Should().Be(" · CloudBlazor");
        config.PageDefaults.DescriptionResult().Should().Be("Default description.");
    }

    [Fact]
    public void AddCloudWeb_IncludesTheCloudBlazorScriptByDefault()
    {
        using ServiceProvider provider = new ServiceCollection().AddCloudWeb().BuildServiceProvider();

        CloudWebConfig config = provider.GetRequiredService<IOptions<CloudWebConfig>>().Value;

        config.IncludeCloudBlazorScript.Should().BeTrue(
            because: "a CloudWeb site must initialize CloudBlazor even with no Blazor script on the page");
    }

    [Fact]
    public void AddCloudWeb_AllowsOptingOutOfTheCloudBlazorScript()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddCloudWeb(config => config.IncludeCloudBlazorScript = false)
            .BuildServiceProvider();

        provider.GetRequiredService<IOptions<CloudWebConfig>>().Value
            .IncludeCloudBlazorScript.Should().BeFalse();
    }

    [Fact]
    public void AddCloudWeb_RejectsNullArguments()
    {
        IServiceCollection services = new ServiceCollection();

        services.Invoking(s => s.AddCloudWeb(null!)).Should().Throw<ArgumentNullException>();
    }

    // ── AddCloudApp ───────────────────────────────────────────────────────

    [Fact]
    public void AddCloudApp_RegistersTheWebNavigationService()
    {
        ServiceDescriptor descriptor = new ServiceCollection()
            .AddCloudApp()
            .Should().ContainSingle(service => service.ServiceType == typeof(INavigationService))
            .Subject;

        descriptor.ImplementationType.Should().Be<WebNavigationService>();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped,
            because: "the service tracks page hierarchy for one client circuit");
    }

    [Fact]
    public void AddCloudApp_RejectsNullArguments()
    {
        Action registerOnNull = () => ((IServiceCollection)null!).AddCloudApp();

        registerOnNull.Should().Throw<ArgumentNullException>();
    }
}

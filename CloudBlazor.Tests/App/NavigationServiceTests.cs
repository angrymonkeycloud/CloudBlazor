using AngryMonkey.CloudBlazor.App;
using FluentAssertions;
using Xunit;

namespace CloudBlazor.Tests.App;

public class NavigationServiceTests
{
    private static WebNavigationService CreateService(
        out TestJSRuntime jsRuntime,
        out TestNavigationManager navigationManager,
        string relativePath = "")
    {
        jsRuntime = new TestJSRuntime();
        navigationManager = new TestNavigationManager(relativePath: relativePath);

        return new WebNavigationService(jsRuntime, navigationManager);
    }

    // ── Page hierarchy ────────────────────────────────────────────────────

    [Fact]
    public void CurrentPage_StartsAtHome()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.CurrentPage.Should().Be(NavigationServiceBase.HomePage);
    }

    [Fact]
    public void ShouldShowBackButton_IsFalseOnHome()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.ShouldShowBackButton.Should().BeFalse();
    }

    [Fact]
    public void ShouldShowBackButton_IsTrueAwayFromHome()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.SetCurrentPage("Details");

        service.ShouldShowBackButton.Should().BeTrue();
    }

    [Fact]
    public void SetCurrentPage_RaisesOnPageChanged()
    {
        WebNavigationService service = CreateService(out _, out _);

        List<string> changes = [];
        service.OnPageChanged += changes.Add;

        service.SetCurrentPage("Details");

        changes.Should().Equal("Details");
    }

    [Fact]
    public void SetCurrentPage_DoesNotRaiseForTheSamePage()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.SetCurrentPage("Details");

        List<string> changes = [];
        service.OnPageChanged += changes.Add;

        service.SetCurrentPage("Details");
        service.SetCurrentPage("DETAILS");

        changes.Should().BeEmpty(because: "page comparison ignores case and only changes raise the event");
    }

    [Fact]
    public void IsCurrentPage_IgnoresCase()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.IsCurrentPage("home").Should().BeTrue();
    }

    [Fact]
    public void SetCurrentPage_RejectsNull()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.Invoking(s => s.SetCurrentPage(null!)).Should().Throw<ArgumentNullException>();
    }

    // ── URI helpers ───────────────────────────────────────────────────────

    [Fact]
    public void PathUri_IsRootedAndBaseRelative()
    {
        WebNavigationService service = CreateService(out _, out _, relativePath: "app/navigation");

        service.PathUri.Should().Be("/app/navigation");
    }

    [Fact]
    public void ToBaseRelativePath_StripsTheBaseUri()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.ToBaseRelativePath("https://example.test/app/navigation").Should().Be("app/navigation");
    }

    [Fact]
    public void ToBaseRelativePath_ReturnsEmptyForAnUnrelatedUri()
    {
        WebNavigationService service = CreateService(out _, out _);

        // NavigationManager throws for a URI outside the base; callers route on the
        // result, so it maps to the base path rather than propagating.
        service.ToBaseRelativePath("https://elsewhere.test/other").Should().BeEmpty();
    }

    // ── Routing ───────────────────────────────────────────────────────────

    [Fact]
    public async Task NavigateToAsync_PushesAnEntryNormally()
    {
        WebNavigationService service = CreateService(out _, out TestNavigationManager navigation);

        await service.NavigateToAsync("/web/metadata");

        navigation.LastNavigation.Should().NotBeNull();
        navigation.LastNavigation!.Uri.Should().Be("/web/metadata");
        navigation.LastNavigation.Replace.Should().BeFalse();
    }

    [Fact]
    public async Task NavigateToAsync_ReplacesTheEntryWhileAPopupIsOpen()
    {
        WebNavigationService service = CreateService(out _, out TestNavigationManager navigation);

        service.IsPopupOpen = true;

        await service.NavigateToAsync("/web/metadata");

        navigation.LastNavigation!.Replace.Should().BeTrue(
            because: "a popup must not leave a dead history entry behind when it closes");
    }

    [Fact]
    public async Task NavigateToAsync_ForcesAReloadWhenAsked()
    {
        WebNavigationService service = CreateService(out _, out TestNavigationManager navigation);

        await service.NavigateToAsync("/web/metadata", forceReload: true);

        navigation.LastNavigation!.ForceLoad.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NavigateToAsync_RejectsAnEmptyRoute(string? route)
    {
        WebNavigationService service = CreateService(out _, out _);

        await service.Invoking(s => s.NavigateToAsync(route!)).Should().ThrowAsync<ArgumentException>();
    }

    // ── Back navigation ───────────────────────────────────────────────────

    [Fact]
    public void TryNavigateBack_ReturnsFalseOnHomeWithNoPopup()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.TryNavigateBack().Should().BeFalse(
            because: "a MAUI host reads this to let the hardware back button exit the app");
    }

    [Fact]
    public void TryNavigateBack_ReturnsTrueWhenAPopupIsOpen()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.IsPopupOpen = true;

        service.TryNavigateBack().Should().BeTrue();
    }

    [Fact]
    public async Task NavigateBackAsync_UsesBrowserHistory()
    {
        WebNavigationService service = CreateService(out TestJSRuntime jsRuntime, out _);

        await service.NavigateBackAsync();

        jsRuntime.Invocations.Select(invocation => invocation.Identifier).Should().Contain("history.back");
    }

    [Fact]
    public async Task NavigateBackAsync_FallsBackToTheRootWhenInteropFails()
    {
        WebNavigationService service = CreateService(out TestJSRuntime jsRuntime, out TestNavigationManager navigation);

        jsRuntime.FailingIdentifiers.Add("history.back");

        await service.NavigateBackAsync();

        navigation.LastNavigation!.Uri.Should().Be("/");
        navigation.LastNavigation.ForceLoad.Should().BeTrue();
    }

    // ── External navigation ───────────────────────────────────────────────

    [Fact]
    public async Task NavigateToExternalAsync_PassesTheUrlAsAnArgument()
    {
        WebNavigationService service = CreateService(out TestJSRuntime jsRuntime, out _);

        const string url = "https://angrymonkeycloud.com/path?a=1";

        await service.NavigateToExternalAsync(url);

        TestJSRuntime.InvocationRecord invocation = jsRuntime.Invocations.Should().ContainSingle().Subject;

        invocation.Identifier.Should().Be("window.location.assign");
        invocation.Arguments.Should().Equal([url]);
    }

    [Fact]
    public async Task NavigateToExternalAsync_NeverBuildsScriptFromTheUrl()
    {
        WebNavigationService service = CreateService(out TestJSRuntime jsRuntime, out _);

        // A URL crafted to break out of a concatenated script string. It must travel as
        // data, and the service must never call an evaluating API.
        const string hostileUrl = "https://example.test/'; alert('xss'); //";

        await service.NavigateToExternalAsync(hostileUrl);

        jsRuntime.Invocations.Should().NotContain(invocation => invocation.Identifier == "eval");
        jsRuntime.Invocations.Single().Arguments.Should().Equal([hostileUrl]);
    }

    [Fact]
    public async Task NavigateToExternalAsync_OpensANewTabWithoutWindowOpener()
    {
        WebNavigationService service = CreateService(out TestJSRuntime jsRuntime, out _);

        await service.NavigateToExternalAsync("https://angrymonkeycloud.com", newTab: true);

        TestJSRuntime.InvocationRecord invocation = jsRuntime.Invocations.Should().ContainSingle().Subject;

        invocation.Identifier.Should().Be("window.open");
        invocation.Arguments.Should().Equal(["https://angrymonkeycloud.com", "_blank", "noopener,noreferrer"]);
    }

    [Fact]
    public async Task NavigateToExternalAsync_FallsBackToRoutingWhenInteropFails()
    {
        WebNavigationService service = CreateService(out TestJSRuntime jsRuntime, out TestNavigationManager navigation);

        jsRuntime.FailingIdentifiers.Add("window.location.assign");

        await service.NavigateToExternalAsync("https://angrymonkeycloud.com");

        navigation.LastNavigation!.Uri.Should().Be("https://angrymonkeycloud.com");
        navigation.LastNavigation.ForceLoad.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NavigateToExternalAsync_IgnoresAnEmptyUrl(string? url)
    {
        WebNavigationService service = CreateService(out TestJSRuntime jsRuntime, out TestNavigationManager navigation);

        await service.NavigateToExternalAsync(url!);

        jsRuntime.Invocations.Should().BeEmpty();
        navigation.Navigations.Should().BeEmpty();
    }

    // ── Soft navigation ───────────────────────────────────────────────────

    [Fact]
    public void SoftNavigate_PushesHistoryAndRaisesNavigateRequest()
    {
        WebNavigationService service = CreateService(out TestJSRuntime jsRuntime, out _, relativePath: "app/navigation");

        List<string> requests = [];
        service.NavigateRequest += (_, url) => requests.Add(url);

        service.SoftNavigate("/details/1");

        jsRuntime.Invocations.Select(invocation => invocation.Identifier).Should().Contain("history.pushState");
        requests.Should().Equal("/details/1");
    }

    [Fact]
    public void SoftNavigate_PassesTheServiceAsTheEventSender()
    {
        WebNavigationService service = CreateService(out _, out _);

        object? sender = null;
        service.NavigateRequest += (source, _) => sender = source;

        service.SoftNavigate("/details/1");

        sender.Should().BeSameAs(service);
    }

    [Fact]
    public void SoftNavigate_RejectsAnEmptyUrl()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.Invoking(s => s.SoftNavigate("  ")).Should().Throw<ArgumentException>();
    }

    // ── Platform flags ────────────────────────────────────────────────────

    [Fact]
    public void WebNavigationService_ReportsTheWebPlatform()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.IsWebPlatform.Should().BeTrue();
    }

    [Fact]
    public void WebNavigationService_DoesNotHandleDeepLinks()
    {
        WebNavigationService service = CreateService(out _, out _);

        service.TryHandleDeepLink("myapp://open").Should().BeFalse();
        service.TryHandleDeepLink(new Uri("myapp://open")).Should().BeFalse();
    }
}

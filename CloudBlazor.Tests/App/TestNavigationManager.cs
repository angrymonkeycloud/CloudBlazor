using Microsoft.AspNetCore.Components;

namespace CloudBlazor.Tests.App;

/// <summary>
/// Minimal <see cref="NavigationManager"/> that records navigations instead of
/// performing them.
/// </summary>
internal sealed class TestNavigationManager : NavigationManager
{
    public TestNavigationManager(string baseUri = "https://example.test/", string relativePath = "")
        => Initialize(baseUri, $"{baseUri}{relativePath}");

    public List<NavigationRecord> Navigations { get; } = [];

    public NavigationRecord? LastNavigation => Navigations.Count == 0 ? null : Navigations[^1];

    protected override void NavigateToCore(string uri, NavigationOptions options)
        => Navigations.Add(new NavigationRecord(uri, options.ForceLoad, options.ReplaceHistoryEntry));

    internal sealed record NavigationRecord(string Uri, bool ForceLoad, bool Replace);
}

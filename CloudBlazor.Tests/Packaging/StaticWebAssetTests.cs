using AngryMonkey.CloudBlazor;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace CloudBlazor.Tests.Packaging;

/// <summary>
/// Guards the contract that makes CloudBlazor initialize itself in a consuming
/// application, including when it arrives through another package.
/// </summary>
/// <remarks>
/// <para>
/// Nothing fails at build time when this contract breaks. The browser behaviors simply
/// never start, in every consuming application at once — which is precisely the failure
/// that is hard to notice and expensive to diagnose.
/// </para>
/// <para>
/// Two independent things are asserted: that the JS initializer is still named the way
/// the SDK requires, and that CloudBlazor's static web assets still travel through
/// CloudBlazor.Web and CloudBlazor.App to the application that references them.
/// </para>
/// </remarks>
public class StaticWebAssetTests
{
    private const string CoreAssembly = "AngryMonkey.CloudBlazor";
    private const string WebAssembly = "AngryMonkey.CloudBlazor.Web";
    private const string AppAssembly = "AngryMonkey.CloudBlazor.App";

    /// <summary>
    /// Libraries that must forward CloudBlazor's assets to their own consumers.
    /// </summary>
    public static TheoryData<string> ForwardingLibraries() => new(WebAssembly, AppAssembly);

    // ── Naming convention ─────────────────────────────────────────────────

    [Fact]
    public void PackageId_MatchesAssemblyName()
    {
        string assemblyName = typeof(CloudBlazorAssets).Assembly.GetName().Name!;

        CloudBlazorAssets.PackageId.Should().Be(assemblyName,
            because: "the static web asset base path and the JS initializer file name both derive from the package id");
    }

    [Fact]
    public void InitializerScriptName_FollowsSdkConvention()
    {
        CloudBlazorAssets.InitializerScriptName.Should().Be($"{CloudBlazorAssets.PackageId}.lib.module.js",
            because: "the Blazor SDK only discovers a JS initializer named {PackageId}.lib.module.js");
    }

    [Fact]
    public void AssetPaths_AreRootedInTheContentPath()
    {
        CloudBlazorAssets.ContentRoot.Should().Be($"_content/{CloudBlazorAssets.PackageId}");
        CloudBlazorAssets.ScriptPath.Should().StartWith($"{CloudBlazorAssets.ContentRoot}/");
        CloudBlazorAssets.AutoInitializerScriptPath.Should().StartWith($"{CloudBlazorAssets.ContentRoot}/");
    }

    // ── The core package ships what it advertises ─────────────────────────

    [Fact]
    public void CorePackage_PublishesItsInitializer()
    {
        string[] initializers = [.. InitializersIn(CoreAssembly)];

        initializers.Should().NotBeEmpty(
            because: "without the JS initializer, CloudBlazor never starts in a Blazor host");
    }

    [Theory]
    [InlineData(nameof(CloudBlazorAssets.ScriptPath))]
    [InlineData(nameof(CloudBlazorAssets.AutoInitializerScriptPath))]
    public void CorePackage_PublishesEveryAdvertisedScript(string assetName)
    {
        string assetPath = assetName switch
        {
            nameof(CloudBlazorAssets.ScriptPath) => CloudBlazorAssets.ScriptPath,
            _ => CloudBlazorAssets.AutoInitializerScriptPath
        };

        // The core manifest serves its own assets without the _content prefix, which
        // only appears once another project consumes them.
        string ownPath = assetPath[$"{CloudBlazorAssets.ContentRoot}".Length..];

        StaticWebAssetManifest manifest = StaticWebAssetManifest.Load(CoreAssembly);

        string? file = manifest.ResolveFile(ownPath);

        file.Should().NotBeNull(because: $"{assetPath} is part of the public API and must ship");
        File.Exists(file).Should().BeTrue(because: $"{assetPath} must resolve to a file that exists");
    }

    // ── Propagation through the dependent packages ────────────────────────

    [Theory]
    [MemberData(nameof(ForwardingLibraries))]
    public void DependentLibrary_ForwardsTheInitializer(string assemblyName)
    {
        string[] initializers = [.. InitializersIn(assemblyName)];

        initializers.Should().NotBeEmpty(
            because: $"an application that references only {assemblyName} still has to receive " +
                     "CloudBlazor's JS initializer, otherwise the behaviors never start");
    }

    [Theory]
    [MemberData(nameof(ForwardingLibraries))]
    public void DependentLibrary_ForwardsTheBehaviorScripts(string assemblyName)
    {
        IEnumerable<string> paths = StaticWebAssetManifest.Load(assemblyName).AssetPaths();

        foreach (string assetPath in new[] { CloudBlazorAssets.ScriptPath, CloudBlazorAssets.AutoInitializerScriptPath })
            paths.Should().Contain($"/{assetPath}",
                because: $"{assemblyName} must forward {assetPath} for hosts that load it explicitly");
    }

    [Theory]
    [MemberData(nameof(ForwardingLibraries))]
    public void DependentLibrary_ListsTheInitializerInItsModuleManifest(string assemblyName)
    {
        StaticWebAssetManifest manifest = StaticWebAssetManifest.Load(assemblyName);

        // This is the file the Blazor runtime reads to decide which initializers to
        // invoke, so it is the closest thing to asserting the runtime behaviour itself.
        string? modulesManifestFile = manifest.ResolveFile($"/{assemblyName}.modules.json");

        modulesManifestFile.Should().NotBeNull(
            because: $"{assemblyName} must publish a JS module manifest");

        string[] modules = JsonSerializer.Deserialize<string[]>(File.ReadAllText(modulesManifestFile!)) ?? [];

        modules.Should().Contain(
            module => module.StartsWith(CloudBlazorAssets.ContentRoot, StringComparison.Ordinal)
                      && module.EndsWith(".lib.module.js", StringComparison.Ordinal),
            because: $"the module manifest of {assemblyName} must list CloudBlazor's initializer");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Initializer paths in a library's manifest. Build output fingerprints the file
    /// name, so the fixed prefix and suffix are matched rather than the exact name.
    /// </summary>
    private static IEnumerable<string> InitializersIn(string assemblyName)
    {
        StaticWebAssetManifest.Exists(assemblyName).Should().BeTrue(
            because: $"{assemblyName} must drop a static web asset manifest into the consuming project's output");

        return StaticWebAssetManifest.Load(assemblyName)
            .AssetPaths()
            .Where(path => path.Contains(CloudBlazorAssets.PackageId, StringComparison.Ordinal)
                           && path.EndsWith(".lib.module.js", StringComparison.Ordinal));
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CloudBlazor.Tests.Packaging;

/// <summary>
/// Reads the <c>{assembly}.staticwebassets.runtime.json</c> manifests that referenced
/// Razor Class Libraries drop into the consuming project's build output.
/// </summary>
/// <remarks>
/// This manifest is how the framework resolves a request path to a file on disk, which
/// makes it the right thing to assert against: it reflects what an application will
/// actually be able to serve.
/// </remarks>
internal sealed class StaticWebAssetManifest
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [JsonPropertyName("ContentRoots")]
    public string[] ContentRoots { get; init; } = [];

    [JsonPropertyName("Root")]
    public ManifestNode Root { get; init; } = new();

    /// <summary>
    /// Directory the test assembly runs from, which is also where referenced libraries
    /// place their manifests.
    /// </summary>
    public static string OutputDirectory { get; } =
        Path.GetDirectoryName(typeof(StaticWebAssetManifest).Assembly.Location)!;

    public static string PathFor(string assemblyName) =>
        Path.Combine(OutputDirectory, $"{assemblyName}.staticwebassets.runtime.json");

    public static bool Exists(string assemblyName) => File.Exists(PathFor(assemblyName));

    public static StaticWebAssetManifest Load(string assemblyName)
    {
        string path = PathFor(assemblyName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"No static web asset manifest for '{assemblyName}'. Expected it at '{path}'.", path);

        return JsonSerializer.Deserialize<StaticWebAssetManifest>(File.ReadAllText(path), _serializerOptions)
            ?? throw new InvalidOperationException($"The static web asset manifest for '{assemblyName}' could not be read.");
    }

    /// <summary>
    /// Every servable asset path, as the application would request it. Pre-compressed
    /// duplicates are left out.
    /// </summary>
    public IEnumerable<string> AssetPaths()
    {
        foreach ((string path, _) in Flatten(Root, string.Empty))
            if (!path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".br", StringComparison.OrdinalIgnoreCase))
                yield return path;
    }

    /// <summary>
    /// Resolves an asset path to the file backing it.
    /// </summary>
    public string? ResolveFile(string assetPath)
    {
        foreach ((string path, ManifestAsset asset) in Flatten(Root, string.Empty))
            if (string.Equals(path, assetPath, StringComparison.OrdinalIgnoreCase))
                return Path.Combine(ContentRoots[asset.ContentRootIndex], asset.SubPath);

        return null;
    }

    private static IEnumerable<(string Path, ManifestAsset Asset)> Flatten(ManifestNode node, string prefix)
    {
        if (node.Children is null)
            yield break;

        foreach ((string name, ManifestNode child) in node.Children)
        {
            string path = $"{prefix}/{name}";

            if (child.Asset is not null)
                yield return (path, child.Asset);

            foreach ((string Path, ManifestAsset Asset) descendant in Flatten(child, path))
                yield return descendant;
        }
    }

    internal sealed class ManifestNode
    {
        [JsonPropertyName("Children")]
        public Dictionary<string, ManifestNode>? Children { get; init; }

        [JsonPropertyName("Asset")]
        public ManifestAsset? Asset { get; init; }
    }

    internal sealed class ManifestAsset
    {
        [JsonPropertyName("ContentRootIndex")]
        public int ContentRootIndex { get; init; }

        [JsonPropertyName("SubPath")]
        public string SubPath { get; init; } = string.Empty;
    }
}

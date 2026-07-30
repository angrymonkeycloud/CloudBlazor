using Microsoft.AspNetCore.Components;

namespace AngryMonkey.CloudBlazor.Web;

public partial class CloudBundleTag
{
    /// <summary>
    /// The bundle to render.
    /// </summary>
    [Parameter, EditorRequired] public required CloudBundle Bundle { get; set; }

    private bool IsExternal => Bundle.Source.StartsWith("http", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The complete tag, or <c>null</c> when the source is not a stylesheet or script.
    /// Returning null keeps an unsupported path from emitting a broken tag.
    /// </summary>
    private string? Result
    {
        get
        {
            if (string.IsNullOrEmpty(Bundle.Source) || !Bundle.Source.Contains('.'))
                return null;

            SourceTypes? sourceType = ResolveSourceType();

            if (sourceType is null)
                return null;

            bool isStylesheet = sourceType == SourceTypes.Css;

            List<string> segments =
            [
                isStylesheet ? "<link" : "<script",
                isStylesheet ? $"href=\"{SourceResult}\"" : $"src=\"{SourceResult}\""
            ];

            segments.AddRange(AdditionalAttributes(sourceType.Value));

            segments.Add(isStylesheet ? "rel=\"stylesheet\">" : "></script>");

            return string.Join(" ", segments);
        }
    }

    private SourceTypes? ResolveSourceType() =>
        Bundle.Source.Split('.').Last().Trim().ToLowerInvariant() switch
        {
            "css" => SourceTypes.Css,
            "js" => SourceTypes.Js,
            _ => null,
        };

    /// <summary>
    /// Applies the minified variant and the cache-busting version.
    /// </summary>
    private string SourceResult
    {
        get
        {
            string source = Bundle.Source;

            if (Bundle.MinOnRelease && !source.Contains(".min.", StringComparison.OrdinalIgnoreCase))
            {
                List<string> segments = [.. source.Split('.')];

                segments.Insert(segments.Count - 1, "min");

                source = string.Join('.', segments);
            }

            if (!Bundle.AppendVersion)
                return source;

            if (IsExternal)
                return source;

            if (Bundle.UseMapping)
                return Assets[source];

            string? baseDirectory = cloudWeb.Value.StaticFilesBaseDirectory;

            // IFileVersionProvider resolves against the web root, so a configured base
            // directory is stripped before the lookup and restored afterwards.
            if (!string.IsNullOrEmpty(baseDirectory))
                source = source.Replace($"{baseDirectory.Trim('/')}/", string.Empty);

            source = fileVersionProvider.AddFileVersionToPath("/", source);

            if (!string.IsNullOrEmpty(baseDirectory))
                source = $"{baseDirectory}/{source}";

            return source;
        }
    }

    private List<string> AdditionalAttributes(SourceTypes sourceType)
    {
        List<string> segments = [];

        if (sourceType == SourceTypes.Js)
        {
            if (Bundle.Defer)
                segments.Add("defer");

            if (Bundle.Async)
                segments.Add("async");
        }

        if (!string.IsNullOrEmpty(Bundle.AddOns))
            segments.Add(Bundle.AddOns);

        return segments;
    }

    private enum SourceTypes
    {
        Js,
        Css,
    }
}

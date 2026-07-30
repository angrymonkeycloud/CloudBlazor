using Microsoft.AspNetCore.Components;

namespace AngryMonkey.CloudWeb;

public partial class CloudBundle
{
    [Parameter] public required string Source { get; set; }
    [Parameter] public bool MinOnRelease { get; set; } = true;
    [Parameter] public string? AddOns { get; set; }
    [Parameter] public bool Defer { get; set; } = true;
    [Parameter] public bool Async { get; set; } = false;
    [Parameter] public bool AppendVersion { get; set; } = true;
    [Parameter] public bool UseMapping { get; set; } = true;

    protected SourceTypes? SourceType { get; set; }

    bool IsExternal => Source.StartsWith("http", StringComparison.OrdinalIgnoreCase);

    private string? Result
    {
        get
        {
            if (string.IsNullOrEmpty(Source) || !Source.Contains('.'))
                return null;

            SourceType = Source.Split('.').Last().Trim().ToLower() switch
            {
                "css" => SourceTypes.CSS,
                "js" => SourceTypes.JS,
                _ => null,
            };

            if (SourceType == null)
                return null;

            List<string> segments = [SourceType == SourceTypes.CSS ? "<link" : "<script"];

            segments.Add(SourceType == SourceTypes.CSS ? $"href=\"{SourceResult}\"" : $"src=\"{SourceResult}\"");

            segments.AddRange(AdditionalResult);

            segments.Add(SourceType == SourceTypes.CSS ? "rel=\"stylesheet\">" : "></script>");

            return string.Join(" ", segments);

            //return Source.Split('.').Last().Trim().ToLower() switch
            //{
            //    "css" => $"<link href=\"{source}\" rel=\"stylesheet\">",
            //    "js" => $"<script src=\"{source}\" {(Defer ? "defer" : null)} {(Async ? "async" : null)}></script>",
            //    _ => null,
            //};
        }
    }

    protected string SourceResult
    {
        get
        {
            string source = Source;

            if (MinOnRelease && !source.Contains(".min.", StringComparison.OrdinalIgnoreCase))
            {
                List<string> sourceSplitted = [.. Source.Split('.')];

                sourceSplitted.Insert(sourceSplitted.Count - 1, "min");
                source = string.Join('.', sourceSplitted);
            }

            if (!IsExternal && AppendVersion && !UseMapping)
            {
                if (!string.IsNullOrEmpty(cloudWeb.Value.StaticFilesBaseDirectory))
                    source = source.Replace($"{cloudWeb.Value.StaticFilesBaseDirectory.Trim('/')}/", string.Empty);

                source = fileVersionProvider.AddFileVersionToPath("/", source);

                if (!string.IsNullOrEmpty(cloudWeb.Value.StaticFilesBaseDirectory))
                    source = $"{cloudWeb.Value.StaticFilesBaseDirectory}/{source}";
            }
            else if (AppendVersion) source = Assets[source];

            return source;
        }
    }

    protected List<string> AdditionalResult
    {
        get
        {
            List<string> segments = [];

            if (SourceType == SourceTypes.JS)
            {
                if (Defer)
                    segments.Add("defer");

                if (Async)
                    segments.Add("async");
            }

            if (!string.IsNullOrEmpty(AddOns))
                segments.Add(AddOns);

            return segments;
        }
    }

    protected enum SourceTypes
    {
        JS,
        CSS,
    }
}
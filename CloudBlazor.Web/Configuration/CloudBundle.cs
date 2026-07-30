namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// A CSS or JavaScript asset to inject into the page head.
/// </summary>
/// <remarks>
/// A plain model rather than a component. It used to be both, which meant every
/// application that configured a bundle got BL0005 warnings for assigning component
/// parameters from its own code. <see cref="CloudBundleTag"/> renders it.
/// </remarks>
public class CloudBundle
{
    /// <summary>
    /// Relative path, or an absolute <c>http(s)</c> URL. Only <c>.css</c> and
    /// <c>.js</c> sources render; anything else is ignored.
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// Inserts <c>.min.</c> before the file extension outside Development.
    /// </summary>
    public bool MinOnRelease { get; set; } = true;

    /// <summary>
    /// Attribute string appended verbatim to the rendered tag, for values such as
    /// <c>integrity</c> or <c>crossorigin</c>.
    /// </summary>
    public string? AddOns { get; set; }

    /// <summary>
    /// Adds <c>defer</c> to <c>&lt;script&gt;</c> tags.
    /// </summary>
    public bool Defer { get; set; } = true;

    /// <summary>
    /// Adds <c>async</c> to <c>&lt;script&gt;</c> tags.
    /// </summary>
    public bool Async { get; set; }

    /// <summary>
    /// Appends a content-based version for cache busting.
    /// </summary>
    public bool AppendVersion { get; set; } = true;

    /// <summary>
    /// Resolves the path through the static asset manifest. When <c>false</c>, the
    /// version comes from <c>IFileVersionProvider</c> instead.
    /// </summary>
    public bool UseMapping { get; set; } = true;
}

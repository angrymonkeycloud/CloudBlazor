namespace AngryMonkey.CloudBlazor;

/// <summary>
/// Static web asset paths published by the <c>AngryMonkey.CloudBlazor</c> package.
/// </summary>
/// <remarks>
/// <para>
/// In a Blazor host that loads <c>blazor.web.js</c>, <c>blazor.server.js</c> or
/// <c>blazor.webassembly.js</c>, CloudBlazor initializes itself through its JS
/// initializer and none of these paths are needed.
/// </para>
/// <para>
/// Hosts without a Blazor script — plain MVC, Razor Pages, or a statically rendered
/// site with no Blazor runtime — have no JS initializer pipeline at all. Those hosts
/// reference <see cref="AutoInitializerScriptPath"/> directly, either through the
/// <c>CloudBlazorScript</c> component or by letting
/// <c>AngryMonkey.CloudBlazor.Web</c> inject it.
/// </para>
/// </remarks>
public static class CloudBlazorAssets
{
    /// <summary>
    /// Package identifier, which is also the static web asset base path segment and
    /// the prefix of the JS initializer file name.
    /// </summary>
    public const string PackageId = "AngryMonkey.CloudBlazor";

    /// <summary>
    /// Root of the package's static web assets as served by the host application.
    /// </summary>
    public const string ContentRoot = $"_content/{PackageId}";

    /// <summary>
    /// The Blazor JS initializer. Discovered automatically by the Blazor runtime;
    /// the file name must stay in sync with <see cref="PackageId"/> or the runtime
    /// will not pick it up.
    /// </summary>
    public const string InitializerScriptName = $"{PackageId}.lib.module.js";

    /// <summary>
    /// ES module exporting CloudBlazor's browser behaviors.
    /// </summary>
    public const string ScriptPath = $"{ContentRoot}/scripts/cloud-blazor.js";

    /// <summary>
    /// ES module that imports <see cref="ScriptPath"/> and initializes CloudBlazor
    /// as soon as the document is ready. Safe to load alongside the JS initializer:
    /// initialization is idempotent.
    /// </summary>
    public const string AutoInitializerScriptPath = $"{ContentRoot}/scripts/cloud-blazor.auto.js";
}

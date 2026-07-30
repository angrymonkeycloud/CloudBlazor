namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// Razor section names used by CloudBlazor.Web.
/// </summary>
public static class CloudWebSections
{
    /// <summary>
    /// Section that <c>CloudHeadContent</c> writes the managed <c>&lt;head&gt;</c>
    /// content into, and that <c>CloudHeadPlaceholder</c> renders it at.
    /// </summary>
    /// <remarks>
    /// Prefer <c>&lt;CloudHeadPlaceholder /&gt;</c> over spelling the section name out
    /// in markup; the constant exists for applications that need the raw value.
    /// </remarks>
    public const string Head = "CloudWeb";
}

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// MVC view helpers for appending bundles to the page published by
/// <see cref="CloudController"/>.
/// </summary>
/// <remarks>
/// Add <c>@using AngryMonkey.CloudBlazor.Web</c> to <c>_ViewImports.cshtml</c> to use
/// <c>@Html.Bundle(...)</c> in views.
/// </remarks>
public static class CloudPageExtensions
{
    /// <summary>
    /// Key that <see cref="CloudController"/> stores the active page under.
    /// </summary>
    internal const string ViewDataKey = "CloudPageStatic";

    /// <summary>
    /// Resolves the page published by <see cref="CloudController"/>. Returns an empty
    /// page when the controller did not publish one, so views render rather than throw.
    /// </summary>
    public static CloudPage Current(ViewDataDictionary viewData)
    {
        ArgumentNullException.ThrowIfNull(viewData);

        return viewData[ViewDataKey] as CloudPage ?? new();
    }

    /// <summary>
    /// Appends a bundle by path to the current page.
    /// </summary>
    public static void Bundle(this IHtmlHelper html, string file)
    {
        ArgumentNullException.ThrowIfNull(html);

        Current(html.ViewData).AppendBundle(file);
    }

    /// <summary>
    /// Appends a fully configured bundle to the current page.
    /// </summary>
    public static void Bundle(this IHtmlHelper html, CloudBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(html);

        Current(html.ViewData).AppendBundle(bundle);
    }
}

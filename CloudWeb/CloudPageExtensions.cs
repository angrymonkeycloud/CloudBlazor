using AngryMonkey.CloudWeb;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class CloudPageExtension
{
    public static CloudPage Current(ViewDataDictionary viewData)
    {
        object? obj = viewData["CloudPageStatic"];

        return obj == null ? new() : obj as CloudPage;
    }

    public static void Bundle(this IHtmlHelper html, string file)
    {
        Current(html.ViewData).AppendBundle(file);
    }

    public static void Bundle(this IHtmlHelper html, CloudBundle bundle)
    {
        Current(html.ViewData).AppendBundle(bundle);
    }
}

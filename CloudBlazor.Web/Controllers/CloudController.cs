using Microsoft.AspNetCore.Mvc;

namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// Base controller that publishes a <see cref="CloudPage"/> to MVC views.
/// </summary>
public class CloudController(CloudPage cloudPage) : Controller
{
    /// <summary>
    /// Publishes the request's page to <c>ViewData</c> and returns it for further
    /// fluent configuration.
    /// </summary>
    /// <param name="title">Optional page title.</param>
    [NonAction]
    public CloudPage CloudPage(string? title = null)
    {
        if (!string.IsNullOrEmpty(title))
            cloudPage.SetTitle(title);

        if (CloudWebConfig.IsNonProductionHost(Request.Host.Host))
        {
            cloudPage.SetIndexPage(false);
            cloudPage.SetFollowPage(false);
        }

        cloudPage.OnModified += PublishPage;

        PublishPage();

        return cloudPage;

        // Indexer rather than Add: calling CloudPage() more than once in a request
        // must not throw on a duplicate key.
        void PublishPage() => ViewData[CloudPageExtensions.ViewDataKey] = cloudPage;
    }

    /// <summary>
    /// Indicates whether the current request comes from a known crawler.
    /// </summary>
    [NonAction]
    public bool IsCrawler() => CloudWebConfig.IsCrawler(ControllerContext.HttpContext.Request.Headers.UserAgent.ToString());
}

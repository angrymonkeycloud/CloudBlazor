using Microsoft.AspNetCore.Mvc;

namespace AngryMonkey.CloudWeb;

public class CloudController(CloudPage cloudPage) : Controller
{
    [NonAction]
    public CloudPage CloudPage(string? title = null)
    {
        if (!string.IsNullOrEmpty(title))
            cloudPage.SetTitle(title);

        string host = Request.Host.Host.ToLower();

        string[] robotsBlockUrls =
        [
            "azurewebsites.net"
        ];

        if (robotsBlockUrls.Any(host.EndsWith))
        {
            cloudPage.SetIndexPage(false);
            cloudPage.SetFollowPage(false);
        }

        cloudPage.OnModified += () => { ViewData["CloudPageStatic"] = cloudPage; };

        ViewData.Add("CloudPageStatic", cloudPage);

        return cloudPage;
    }

    [NonAction]
    public bool IsCrawler()
    {
        string userAgeny = ControllerContext.HttpContext.Request.Headers.UserAgent.ToString().Trim().ToLower();

        return CloudWebConfig.CrawlersUserAgents.Any(userAgeny.Contains);
    }
}

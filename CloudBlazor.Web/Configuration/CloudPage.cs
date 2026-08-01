using Microsoft.AspNetCore.Http;
using System.Collections.ObjectModel;
using System.Text;

namespace AngryMonkey.CloudBlazor.Web;

public partial class CloudPage
{
    private readonly bool _isNonProductionHost;

    public CloudPage() => IsCrawler = false;

    public CloudPage(IHttpContextAccessor accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        HttpRequest? request = accessor.HttpContext?.Request;

        IsCrawler = CloudWebConfig.IsCrawler(request?.Headers.UserAgent.ToString());

        // Preview and staging hosts must never be indexed.
        _isNonProductionHost = CloudWebConfig.IsNonProductionHost(request?.Host.Host);

        if (_isNonProductionHost)
        {
            SetIndexPage(false);
            SetFollowPage(false);
        }
    }

    public string? Title { get; internal set; }
    public string? Keywords { get; internal set; }
    public string? Description { get; internal set; }
    public bool? IndexPage { get; internal set; }
    public bool? FollowPage { get; internal set; }

    /// <summary>Prevents search engines from showing a cached copy of the page.</summary>
    public bool? NoArchive { get; internal set; }

    /// <summary>Largest image preview a search engine may show.</summary>
    public CloudMaxImagePreviews? MaxImagePreview { get; internal set; }

    /// <summary>Maximum snippet length in characters. <c>-1</c> lifts the limit, <c>0</c> suppresses snippets.</summary>
    public int? MaxSnippet { get; internal set; }

    /// <summary>Maximum video preview length in seconds. <c>-1</c> lifts the limit.</summary>
    public int? MaxVideoPreview { get; internal set; }
    public string? Favicon { get; internal set; }
    public string? ThemeColor { get; internal set; }
    public string? Manifest { get; internal set; }

    public bool? AddLegacyExportsCreation { get; internal set; }

    public bool IsCrawler { get; internal set; }

    internal readonly List<string> _titleAddOns = [];
    public ReadOnlyCollection<string> TitleAddOns => _titleAddOns.AsReadOnly();

    internal readonly List<CloudPageFeatures> _features = [];
    public ReadOnlyCollection<CloudPageFeatures> Features => _features.AsReadOnly();

    internal readonly List<CloudBundle> _bundles = [];
    public ReadOnlyCollection<CloudBundle> Bundles => _bundles.AsReadOnly();

    internal readonly List<CloudHeadLink> _headLinks = [];

    /// <summary>Icons, resource hints, and other reusable document-head links.</summary>
    public ReadOnlyCollection<CloudHeadLink> HeadLinks => _headLinks.AsReadOnly();

    public event Action? OnModified;

    /// <summary>
    /// Clears route-specific state while retaining request-derived safety settings.
    /// CloudBlazor.Web calls this automatically before interactive navigation renders
    /// the next route, preventing metadata and JSON-LD from leaking between pages.
    /// </summary>
    public CloudPage Reset()
    {
        Title = null;
        Keywords = null;
        Description = null;
        IndexPage = _isNonProductionHost ? false : null;
        FollowPage = _isNonProductionHost ? false : null;
        NoArchive = null;
        MaxImagePreview = null;
        MaxSnippet = null;
        MaxVideoPreview = null;
        Favicon = null;
        ThemeColor = null;
        Manifest = null;
        AddLegacyExportsCreation = null;

        Canonical = null;
        OpenGraphType = null;
        SiteName = null;
        SocialTitle = null;
        SocialDescription = null;
        Image = null;
        Locale = null;
        TwitterCard = null;
        TwitterSite = null;
        TwitterCreator = null;

        _titleAddOns.Clear();
        _features.Clear();
        _bundles.Clear();
        _headLinks.Clear();
        _alternates.Clear();
        _localeAlternates.Clear();
        _structuredData.Clear();

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Adds an icon, preload, preconnect, or other link to the document head.</summary>
    public CloudPage AddHeadLink(CloudHeadLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        _headLinks.RemoveAll(existing =>
            string.Equals(existing.Rel, link.Rel, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(existing.Href, link.Href, StringComparison.OrdinalIgnoreCase));

        _headLinks.Add(link);
        OnModified?.Invoke();

        return this;
    }

    /// <summary>Adds document-head links in their render order.</summary>
    public CloudPage AddHeadLinks(params CloudHeadLink[]? links)
    {
        if (links == null)
            return this;

        foreach (CloudHeadLink link in links)
        {
            ArgumentNullException.ThrowIfNull(link);

            _headLinks.RemoveAll(existing =>
                string.Equals(existing.Rel, link.Rel, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.Href, link.Href, StringComparison.OrdinalIgnoreCase));

            _headLinks.Add(link);
        }

        OnModified?.Invoke();

        return this;
    }

    public CloudPage InsertBundle(int index, CloudBundle bundle)
    {
        _bundles.Insert(index, bundle);

        OnModified?.Invoke();

        return this;
    }

    public CloudPage AppendBundle(CloudBundle bundle) => AppendBundles(bundle);

    public CloudPage AppendBundles(params CloudBundle[]? bundles)
    {
        if (bundles == null)
            return this;

        foreach (CloudBundle bundle in bundles)
            _bundles.Add(bundle);

        OnModified?.Invoke();

        return this;
    }

    public CloudPage AppendBundle(string bundle) => AppendBundles(bundle);

    public CloudPage AppendBundles(params string[]? bundles)
    {
        if (bundles == null)
            return this;

        foreach (string bundle in bundles)
            _bundles.Add(new CloudBundle() { Source = bundle });

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetTitle(string title)
    {
        Title = title;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetFavicon(string path)
    {
        Favicon = path;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetThemeColor(string color)
    {
        ThemeColor = color;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetManifest(string path)
    {
        Manifest = path;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetKeywords(string keywords)
    {
        Keywords = keywords;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetDescription(string description)
    {
        Description = description;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetIndexPage(bool indexPage)
    {
        IndexPage = indexPage;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetFollowPage(bool followPage)
    {
        FollowPage = followPage;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Prevents search engines from showing a cached copy of the page.</summary>
    public CloudPage SetNoArchive(bool noArchive)
    {
        NoArchive = noArchive;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the largest image preview a search engine may show.</summary>
    public CloudPage SetMaxImagePreview(CloudMaxImagePreviews maxImagePreview)
    {
        MaxImagePreview = maxImagePreview;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the maximum snippet length. <c>-1</c> lifts the limit, <c>0</c> suppresses snippets.</summary>
    public CloudPage SetMaxSnippet(int maxSnippet)
    {
        MaxSnippet = maxSnippet;

        OnModified?.Invoke();

        return this;
    }

    /// <summary>Sets the maximum video preview length in seconds. <c>-1</c> lifts the limit.</summary>
    public CloudPage SetMaxVideoPreview(int maxVideoPreview)
    {
        MaxVideoPreview = maxVideoPreview;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetTitleAddOns(IEnumerable<string> titleAddOns)
    {
        _titleAddOns.Clear();
        _titleAddOns.AddRange(titleAddOns);

        OnModified?.Invoke();

        return this;
    }

    public CloudPage SetAddLegacyExportsCreation(bool addLegacyExportsCreation)
    {
        AddLegacyExportsCreation = addLegacyExportsCreation;

        OnModified?.Invoke();

        return this;
    }

    public CloudPage AddFeature(CloudPageFeatures feature) => AddFeatures(feature);

    public CloudPage AddFeatures(params CloudPageFeatures[] features)
    {
        _features.AddRange(features);

        OnModified?.Invoke();

        return this;
    }

    public string? RobotsResult()
    {
        List<string> content = [];

        if (IndexPage.HasValue && !IndexPage.Value)
            content.Add("noindex");

        if (FollowPage.HasValue && !FollowPage.Value)
            content.Add("nofollow");

        // A page excluded from the index gains nothing from preview or snippet limits, and
        // pairing them reads as contradictory. noindex wins outright.
        bool indexable = !IndexPage.HasValue || IndexPage.Value;

        if (indexable)
        {
            if (NoArchive == true)
                content.Add("noarchive");

            if (MaxImagePreview.HasValue)
                content.Add($"max-image-preview:{MaxImagePreview.Value switch
                {
                    CloudMaxImagePreviews.None => "none",
                    CloudMaxImagePreviews.Standard => "standard",
                    _ => "large"
                }}");

            if (MaxSnippet.HasValue)
                content.Add($"max-snippet:{MaxSnippet.Value}");

            if (MaxVideoPreview.HasValue)
                content.Add($"max-video-preview:{MaxVideoPreview.Value}");
        }

        if (content.Any())
            return string.Join(", ", content);

        return null;
    }

    public string? TitleResult(CloudWebConfig cloudWeb)
    {
        StringBuilder titleBuilder = new();

        if (string.IsNullOrEmpty(Title))
            titleBuilder.Append(cloudWeb.PageDefaults.Title);
        else
            titleBuilder.Append($"{cloudWeb.TitlePrefix}{Title}{cloudWeb.TitleSuffix}");

        List<string> addOns = [.. cloudWeb.PageDefaults._titleAddOns];
        addOns.AddRange(TitleAddOns);

        if (addOns.Any())
            foreach (string addText in addOns)
                if (titleBuilder.Length + addText.Length + 1 <= 64)
                    titleBuilder.Append($" {addText}");

        return titleBuilder.ToString();
    }

    public string? FaviconResult() => Favicon;

    public string? FaviconTypeResult()
    {
        if (string.IsNullOrWhiteSpace(Favicon))
            return null;

        string extension = Path.GetExtension(Favicon.Split('?', '#')[0]).ToLowerInvariant();

        return extension switch
        {
            ".ico" => "image/x-icon",
            ".svg" => "image/svg+xml",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => null
        };
    }

    public string? ThemeColorResult() => ThemeColor;

    public string? ManifestResult() => Manifest;

    public string? KeywordsResult() => Keywords;

    public string? DescriptionResult()
    {
        if (Description == null || Description.Length <= 160)
            return Description;

        string result = Description[..157].TrimEnd();
        int lastSpace = result.LastIndexOf(' ');

        // Prefer a natural boundary, but keep useful snippet length when a single long
        // token or URL occupies most of the description.
        if (lastSpace >= 120)
            result = result[..lastSpace].TrimEnd();

        return $"{result}...";
    }
}

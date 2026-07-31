namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// The image a link preview shows, shared by Open Graph and Twitter cards.
/// </summary>
public class CloudPageImage
{
    /// <summary>
    /// Absolute URL of the image. Relative paths are resolved against the request origin
    /// when the page renders: social crawlers do not resolve relative URLs.
    /// </summary>
    public required string Url { get; set; }

    /// <summary>Width in pixels. Lets a crawler reserve layout before fetching the file.</summary>
    public int? Width { get; set; }

    /// <summary>Height in pixels.</summary>
    public int? Height { get; set; }

    /// <summary>Alternative text describing the image.</summary>
    public string? Alt { get; set; }

    /// <summary>MIME type, inferred from the extension when not set.</summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// The MIME type to advertise: <see cref="MimeType"/> when supplied, otherwise inferred
    /// from the file extension.
    /// </summary>
    public string? MimeTypeResult()
    {
        if (!string.IsNullOrWhiteSpace(MimeType))
            return MimeType;

        string extension = Path.GetExtension(Url.Split('?', '#')[0]).ToLowerInvariant();

        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => null
        };
    }
}

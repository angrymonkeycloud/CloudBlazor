namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// Largest image size a search engine may show for the page, emitted as the
/// <c>max-image-preview</c> robots directive.
/// </summary>
public enum CloudMaxImagePreviews
{
    /// <summary>No image preview.</summary>
    None,

    /// <summary>A thumbnail at most.</summary>
    Standard,

    /// <summary>Up to the full viewport width. Required for Google Discover eligibility.</summary>
    Large
}

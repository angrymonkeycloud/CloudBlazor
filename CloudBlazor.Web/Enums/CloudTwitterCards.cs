namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// Twitter card layouts, emitted as <c>&lt;meta name="twitter:card" /&gt;</c>.
/// </summary>
public enum CloudTwitterCards
{
    /// <summary>Title, description and a small square thumbnail.</summary>
    Summary,

    /// <summary>Title, description and a full-width image. The usual choice for articles.</summary>
    SummaryLargeImage,

    /// <summary>Promotes an application.</summary>
    App,

    /// <summary>Video or audio player embedded in the timeline.</summary>
    Player
}

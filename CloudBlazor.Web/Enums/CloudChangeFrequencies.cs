namespace AngryMonkey.CloudBlazor.Web;

/// <summary>
/// How often a sitemap entry is expected to change, emitted as <c>&lt;changefreq&gt;</c>.
/// </summary>
/// <remarks>
/// A hint, not an instruction: crawlers weigh their own signals more heavily. Omitting it
/// is perfectly reasonable.
/// </remarks>
public enum CloudChangeFrequencies
{
    /// <summary>Changes on every access.</summary>
    Always,

    /// <summary>Changes roughly hourly.</summary>
    Hourly,

    /// <summary>Changes roughly daily.</summary>
    Daily,

    /// <summary>Changes roughly weekly.</summary>
    Weekly,

    /// <summary>Changes roughly monthly.</summary>
    Monthly,

    /// <summary>Changes roughly yearly.</summary>
    Yearly,

    /// <summary>Archived: not expected to change again.</summary>
    Never
}

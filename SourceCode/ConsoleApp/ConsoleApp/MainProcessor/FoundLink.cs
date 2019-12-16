using System.Diagnostics;

namespace MainProcessor
{
    /// <summary>
    /// Custom class to store the link information.
    /// </summary>
    [DebuggerDisplay("{Title} -> {Link}")]
    public class FoundLink
    {
        /// <summary>
        /// The full match, like "href: link.md" or "[title](link.md)".
        /// </summary>
        public string FullMatch { get; set; }

        /// <summary>
        /// The link.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// The title.
        /// </summary>
        public string Title { get; set; }
    }
}

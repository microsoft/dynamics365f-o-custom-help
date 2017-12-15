namespace CustomPlugin
{
    /// <summary>
    /// Custom class to store the link information.
    /// </summary>
    public class FoundLink
    {
        /// <summary>
        /// The full match, like "href: link.md" or "[title](link.md)"
        /// </summary>
        public string FullMatch;
        /// <summary>
        /// The link.
        /// </summary>
        public string Link;
    }
}

namespace MainProcessor
{
    /// <summary>
    /// Custom class to store the link information.
    /// </summary>
    public class FoundPicture
    {
        /// <summary>
        /// The full match, like "href: link.md" or "[title](link.md)".
        /// </summary>
        public string FullMatch;
        /// <summary>
        /// The link.
        /// </summary>
        public string Link1;
        /// <summary>
        /// The title.
        /// </summary>
        public string Link2;
    }
}

namespace CustomPlugin
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    #endregion

    /// <summary>
    /// Helper for URLs.
    /// </summary>
    public class UrlHelper
    {
        #region Constants        
        /// <summary>
        /// The regex for the links in MD file
        /// </summary>
        private const string _regexMdLinks = @"\[.*?\](\s*)?\((.*?{0})(\s*?)\)";
        /// <summary>
        /// The regex for the links in YAML file
        /// </summary>
        private const string _regexYamlLinks = @"href:(\s*)?(.*{0})";
        #endregion

        #region Public Methods        
        /// <summary>
        /// Finds all links in content.
        /// </summary>
        /// <param name="isYaml">if set to <c>true</c> [is yaml].</param>
        /// <param name="content">The content.</param>
        /// <returns>Array of the links.</returns>
        public static FoundLink[] FindAllLinks(bool isYaml, string content)
        {
            Regex rgx = new Regex(String.Format(isYaml ? _regexYamlLinks : _regexMdLinks, "\\.md"));
            MatchCollection matches = rgx.Matches(content);
            List<FoundLink> urls = new List<FoundLink>();
            for (int i = 0; i < matches.Count; i++)
            {
                if (!matches[i].Value.StartsWith("[!include"))
                {
                    urls.Add(new FoundLink() { FullMatch = matches[i].Value, Link = matches[i].Groups[2].Value });
                }
            }
            return urls.ToArray();
        }

        /// <summary>
        /// Builds the URL from root based on relative paths.
        /// </summary>
        /// <param name="relativeFilePathFromRoot">The relative file path from root.</param>
        /// <param name="relativeLinkPath">The relative link path.</param>
        /// <returns>The URL.</returns>
        public static string BuildFullUrl(string relativeFilePathFromRoot, string relativeLinkPath)
        {
            if (IsLinkRelative(relativeLinkPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(relativeFilePathFromRoot), relativeLinkPath.TrimStart('~').TrimStart('\\').TrimStart('/')));
                return "\\" + dirInfo.FullName.Substring(dirInfo.Root.FullName.Length);
            }
            return relativeLinkPath;
        }

        /// <summary>
        /// Determines whether the specified link is relative.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>
        ///   <c>true</c> if [link] is relative; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLinkRelative(string link)
        {
            return !link.TrimStart('~').StartsWith("/");
        }

        /// <summary>
        /// Gets the query from link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>The query string.</returns>
        public static string GetQueryFromLink(string link)
        {
            int indexOfQuery = link.IndexOf("?");
            if (indexOfQuery > -1)
            {
                return link.Substring(indexOfQuery);
            }
            indexOfQuery = link.IndexOf("#");
            if (indexOfQuery > -1)
            {
                return link.Substring(indexOfQuery);
            }
            return String.Empty;
        }
        #endregion
    }
}

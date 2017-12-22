using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MainProcessor
{
    public class ConceptualLinkProcessor : LinkProcessorBase
    {
        #region Constants

        //private const string RegexMdLinksNew = @"(?<!\[\!)(?<!\[\!include)\[\s?(?!\!)(?!\!include)(.*?)\]\s*?\((.*?[\#?]?)?(\s*?)\)";
        /// <summary>
        /// The regex for the links in MD file (group 1 - title, group 2 - link)
        /// </summary>
        private const string RegexMdLinks = @"(?<!include)\[([\w\d\s\(\-–—:\/,\.„”'""\\#\+&)]*?)\]\s*?\((.*?[\#?]?)?(\s*?)\)";
        /// <summary>
        /// The regex include links (group 2 - title, group 3 - link)
        /// </summary>
        private const string RegexIncludeLinks = @"(!include)\[([\w\d\s\(\-–—:\/,\.„”'""\\#\+&)]*?)\]\s*?\((.*?[\#?]?)?(\s*?)\)";

        //private const string RegexMdPicturesNew = @"\[(!\[(.*?)\]\s*?\((.*?[\#?]?)?(\s*?)\))\]\((.*?)\)";
        /// <summary>
        /// The regex for markdown pictures
        /// </summary>
        private const string RegexMdPictures = @"\[(!\[([\!\w\d\s\(\-\–\—\:\\\/\,\.„”\'""\#\+\&\)]*?)\]\s*?\((.*?[\#?]?)?(\s*?)\))\]\((.*?)\)";
        /// <summary>
        /// The regex for the RAW HTML links in MD file (group 3 - title, group 2 - link)
        /// </summary>
        private const string RegexRawHtmlLinks = @"<\s?a.*?href=([""'])(.*?)\1.*?>(.*?)<\/";
        /// <summary>
        /// The regex raw HTML HTML images (group 3 - title, group 2 - link)
        /// </summary>
        private const string RegexRawHtmlImgs = @"<\s?img.*?src=([""'])(.*?)\1.*?>(.*?)<\/";
        /// <summary>
        /// The regex for the links in YAML file (group 1 - title, group 2 - link)
        /// </summary>
        private const string RegexYamlLinks = @"name:\s?(.*?)\s*?href:(\s*)?(.*\.md.*?)";
        #endregion

        #region Private Fields
        /// <summary>
        /// The content
        /// </summary>
        private string _content;
        /// <summary>
        /// The new content
        /// </summary>
        private readonly StringBuilder _newContent;
        #endregion

        #region Constructor        

        /// <summary>
        /// Initializes a new instance of the <see cref="ConceptualLinkProcessor"/> class.
        /// </summary>
        /// <param name="logger">The ILogger instance</param>
        /// <param name="baseDir">The base dir.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="baseEnUsUrl">The base en-us URL.</param>
        /// <param name="baseWoExtUrl">The base URL for the en-us links without .md extension.</param>
        /// <param name="externalText">The external text.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="enRepository">The en repository.</param>
        /// <param name="content">The content.</param>
        /// <param name="newContent">The new content</param>
        public ConceptualLinkProcessor(ILogger logger, string baseDir, string baseUrl, string baseEnUsUrl, string baseWoExtUrl, string externalText, string sourceFilePath, string enRepository, string content, StringBuilder newContent) : base(logger, baseDir, baseUrl, baseEnUsUrl, baseWoExtUrl, externalText, sourceFilePath, enRepository)
        {
            _content = content;
            _newContent = newContent;
            _newContent.Append(content);
        }
        #endregion

        #region Public Overridden methods
        public override bool ProcessContentLinks()
        {
            string fname = Path.GetFileName(SourceFilePath);
            bool result = (fname != null && fname.Equals("toc.md", StringComparison.InvariantCultureIgnoreCase) || ContentHasAudienceApplicationUser(_content));
            if (result)
            {
                FoundLink[] links = FindAllLinks(false, _content);
                Links.AddRange(links.Select(l => l.FullMatch));
                foreach (FoundLink link in links.GroupBy(k => k).Select(k => k.Key))
                {
                    if (String.IsNullOrEmpty(link.Link.Trim()))
                    {
                        continue;
                    }

                    string linkClear = link.Link;

                    if (linkClear.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ||
                        linkClear.EndsWith("toc.md", StringComparison.InvariantCultureIgnoreCase) ||
                        linkClear.EndsWith("toc.yml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        string href = BuildFullUrl("/" + SourceFilePath, linkClear);
                        ProcessLink(href, link.Link, new ConceptualItemParameter(ref _content, link, href));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex.Message);
                    }
                }

                FoundPicture[] pictures = FindAllPictures(_content);
                Pictures.AddRange(pictures.Select(p => p.FullMatch));
                foreach (FoundPicture picture in pictures)
                {
                    try
                    {
                        string href = BuildFullUrl("/" + SourceFilePath, GetOnlyLink(picture.Link1));
                        ProcessLink(href, picture.Link1, null);

                        if (!String.IsNullOrEmpty(picture.Link2) && !picture.Link1.Equals(picture.Link2))
                        {
                            href = BuildFullUrl("/" + SourceFilePath, GetOnlyLink(picture.Link2));
                            ProcessLink(href, picture.Link2, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex.Message);
                    }
                }

                FoundLink[] includes = FindIncludedLinks(_content);
                foreach (FoundLink link in includes.GroupBy(k => k).Select(k => k.Key))
                {
                    if (String.IsNullOrEmpty(link.Link.Trim()))
                    {
                        continue;
                    }

                    string linkClear = link.Link;

                    if (linkClear.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ||
                        linkClear.EndsWith("toc.md", StringComparison.InvariantCultureIgnoreCase) ||
                        linkClear.EndsWith("toc.yml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        string href = BuildFullUrl("/" + SourceFilePath, linkClear);
                        if (!FilesToIgnore.Contains(href.TrimStart('\\')))
                        {
                            FilesToIgnore.Add(href.TrimStart('\\'));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex.Message);
                    }
                }
                return HasModified;
            }
            if (!(fname != null && fname.Equals("toc.md", StringComparison.InvariantCultureIgnoreCase)))
            {
                FilesToRemove.Add(SourceFilePath.TrimStart('\\'));
            }
            return false;
        }
        #endregion

        #region Protected Overridden methods
        protected override bool ReplaceLink(ref IProcessLinkParameter parameters, LinkType linkType)
        {
            FoundLink link = ((ConceptualItemParameter)parameters).Link;
            string href = ((ConceptualItemParameter)parameters).Href;

            string baseUrl = null;

            switch (linkType)
            {
                case LinkType.GeneralLink:
                    baseUrl = BaseUrl;
                    break;
                case LinkType.EnUsLink:
                    baseUrl = BaseEnUsUrl;
                    break;
                case LinkType.RelativeWoExt:
                    baseUrl = BaseWoExtUrl;
                    break;
            }

            if (baseUrl == null)
            {
                return false;
            }

            if (Uri.TryCreate(new Uri(baseUrl), href.TrimStart('\\').TrimEnd(".md"), out Uri uri))
            {
                string query = GetQueryFromLink(link.Link);
                string newLink = $"{uri.AbsoluteUri}{query}";

                if (linkType == LinkType.EnUsLink)
                {
                    ReplacedEnUsLinks.AppendLine($"{SourceFilePath};{link.Link};{link.Title};{newLink};{link.Title + ExternalText}");
                }
                else
                {
                    ReplacedLinks.AppendLine($"{SourceFilePath};{link.Link};{link.Title};{newLink};{link.Title + ExternalText}");
                    string fileToRemove = href.TrimStart('\\');
                    if (!String.IsNullOrEmpty(query))
                    {
                        fileToRemove = fileToRemove.Replace(query, "");
                    }
                    if (Path.GetExtension(fileToRemove).Equals(".md", StringComparison.InvariantCultureIgnoreCase) && !FilesToRemove.Contains(fileToRemove))
                    {
                        FilesToRemove.Add(fileToRemove);
                    }
                }

                _newContent.Replace(link.FullMatch, link.FullMatch.Replace(link.Title, link.Title + ExternalText).Replace(link.Link, newLink));
                return true;
            }
            Logger.LogWarning($"URI could not be created: {BaseUrl} {href}");
            return false;
        }
        #endregion

        #region Private methods        
        /// <summary>
        /// Finds all links.
        /// </summary>
        /// <param name="isYaml">if set to <c>true</c> [is yaml].</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        private FoundLink[] FindAllLinks(bool isYaml, string content)
        {
            Regex rgx = new Regex(isYaml ? RegexYamlLinks : RegexMdLinks);
            MatchCollection matches = rgx.Matches(content);
            List<FoundLink> urls = new List<FoundLink>();
            for (int i = 0; i < matches.Count; i++)
            {
                string link = matches[i].Groups[2].Value;
                if (!link.StartsWith("#") && !link.StartsWith("http"))
                {
                    urls.Add(new FoundLink()
                    {
                        FullMatch = matches[i].Value,
                        Link = GetOnlyLink(link),
                        Title = matches[i].Groups[1].Value
                    });
                }
            }

            if (!isYaml)
            {
                rgx = new Regex(RegexRawHtmlLinks);
                matches = rgx.Matches(content);
                for (int i = 0; i < matches.Count; i++)
                {
                    string link = matches[i].Groups[2].Value;
                    if (!link.StartsWith("http") && !link.StartsWith("#"))
                    {
                        urls.Add(new FoundLink()
                        {
                            FullMatch = matches[i].Value,
                            Link = link,
                            Title = matches[i].Groups[3].Value
                        });
                    }
                }
            }
            return urls.ToArray();
        }

        /// <summary>
        /// Finds the links which are stored as "!include".
        /// </summary>
        /// <returns></returns>
        private FoundLink[] FindIncludedLinks(string content)
        {
            Regex rgx = new Regex(RegexIncludeLinks);
            MatchCollection matches = rgx.Matches(content);
            List<FoundLink> urls = new List<FoundLink>();
            for (int i = 0; i < matches.Count; i++)
            {
                string link = matches[i].Groups[3].Value;
                if (!link.StartsWith("#") && !link.StartsWith("http"))
                {
                    urls.Add(new FoundLink()
                    {
                        FullMatch = matches[i].Value,
                        Link = GetOnlyLink(link),
                        Title = matches[i].Groups[2].Value
                    });
                }
            }
            return urls.ToArray();
        }

        /// <summary>
        /// Finds all picture links.
        /// </summary>
        /// <returns></returns>
        private FoundPicture[] FindAllPictures(string content)
        {
            Regex rgx = new Regex(RegexMdPictures);
            MatchCollection matches = rgx.Matches(content);
            List<FoundPicture> urls = new List<FoundPicture>();
            for (int i = 0; i < matches.Count; i++)
            {
                string link1 = matches[i].Groups[3].Value;
                string link2 = matches[i].Groups[5].Value;
                if (!link1.StartsWith("#") && !link1.StartsWith("http"))
                {
                    urls.Add(new FoundPicture()
                    {
                        FullMatch = matches[i].Value,
                        Link1 = link1,
                        Link2 = link2
                    });
                }
            }

            rgx = new Regex(RegexRawHtmlImgs);
            matches = rgx.Matches(content);
            for (int i = 0; i < matches.Count; i++)
            {
                string link = matches[i].Groups[2].Value;
                if (!link.StartsWith("http") && !link.StartsWith("#"))
                {
                    urls.Add(new FoundPicture()
                    {
                        FullMatch = matches[i].Value,
                        Link1 = link,
                        Link2 = ""
                    });
                }
            }
            return urls.ToArray();
        }

        /// <summary>
        /// Gets the link from a little bit incorrect text, like (/some/path/file.md "Additional text is going here").
        /// </summary>
        /// <returns></returns>
        private string GetOnlyLink(string link)
        {
            int indexOfQuote1 = link.IndexOf("\"", StringComparison.InvariantCultureIgnoreCase);
            int indexOfQuote2 = link.IndexOf("'", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfQuote1 > -1 || indexOfQuote2 > -1)
            {
                int min = 0;
                if (indexOfQuote1 < indexOfQuote2)
                {
                    if (indexOfQuote1 > -1)
                    {
                        min = indexOfQuote1;
                    }
                    else
                    {
                        min = indexOfQuote2;
                    }
                }
                else if (indexOfQuote2 < indexOfQuote1)
                {
                    if (indexOfQuote2 > -1)
                    {
                        min = indexOfQuote2;
                    }
                    else
                    {
                        min = indexOfQuote1;
                    }
                }

                return link.Substring(0, min).Trim();
            }
            
            return link;
        }

        /// <summary>
        /// Builds the full URL (replaces the dots ../.. and etc.)
        /// </summary>
        /// <param name="sourceFilePathFromRoot">The source file path from root.</param>
        /// <param name="relativeLinkPath">The relative link path.</param>
        /// <returns></returns>
        private string BuildFullUrl(string sourceFilePathFromRoot, string relativeLinkPath)
        {
            if (IsLinkRelative(relativeLinkPath))
            {
                string relPath = Path.GetDirectoryName(sourceFilePathFromRoot);
                if (relPath != null)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(relPath, relativeLinkPath.TrimStart('~').TrimStart('\\').TrimStart('/')));
                    return "\\" + dirInfo.FullName.Substring(dirInfo.Root.FullName.Length);
                }
            }
            return relativeLinkPath;
        }

        /// <summary>
        /// Gets the query string from the link.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns></returns>
        private string GetQueryFromLink(string link)
        {
            int indexOfQuery = link.IndexOf("?", StringComparison.Ordinal);
            if (indexOfQuery > -1)
            {
                return link.Substring(indexOfQuery);
            }
            indexOfQuery = link.IndexOf("#", StringComparison.Ordinal);
            if (indexOfQuery > -1)
            {
                return link.Substring(indexOfQuery);
            }
            return String.Empty;
        }

        /// <summary>
        /// Determines whether the specified link is relative.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <returns>
        ///   <c>true</c> if [link] is relative; otherwise, <c>false</c>.
        /// </returns>
        private bool IsLinkRelative(string link)
        {
            return !link.TrimStart('~').StartsWith("/");
        }
        #endregion

        internal class ConceptualItemParameter: IProcessLinkParameter
        {
            public string Content;
            public FoundLink Link;
            public string Href;

            public ConceptualItemParameter(ref string content, FoundLink link, string href)
            {
                Content = content;
                Link = link;
                Href = href;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MainProcessor
{
    /// <summary>
    /// The base class for processing TOC and Conceptual files.
    /// </summary>
    public abstract class LinkProcessorBase : ILinkProcessor
    {
        #region Constants        
        /// <summary>
        /// The pattern for the meta block
        /// </summary>
        private const string PatternMeta = @"(---)([.\s\w\W]*)(---)";
        /// <summary>
        /// The pattern for 'audience: Application User'
        /// </summary>
        private const string PatternAudience = @"(audience)\s*?:\s*?.*?(Application User)";

        private const string PatternRedirect = @"redirect_url";
        #endregion

        #region Protected Fields        

        protected ILogger Logger;
        /// <summary>
        /// True if content has been modified
        /// </summary>
        protected bool HasModified;
        /// <summary>
        /// The log of replaced links
        /// </summary>
        protected StringBuilder ReplacedLinks = new StringBuilder();
        /// <summary>
        /// The replaced en us links
        /// </summary>
        protected StringBuilder ReplacedEnUsLinks = new StringBuilder();
        /// <summary>
        /// The base dir
        /// </summary>
        protected string BaseDir;
        /// <summary>
        /// The base URL
        /// </summary>
        protected string BaseUrl;
        /// <summary>
        /// The base URL
        /// </summary>
        protected string BaseEnUsUrl;
        /// <summary>
        /// The base w/o extenstion URL
        /// </summary>
        protected string BaseWoExtUrl;
        /// <summary>
        /// The external text
        /// </summary>
        protected string ExternalText;
        /// <summary>
        /// The path of en-US repository
        /// </summary>
        protected string EnRepository;
        /// <summary>
        /// The source file path
        /// </summary>
        protected string SourceFilePath;
        /// <summary>
        /// The files to remove
        /// </summary>
        protected List<string> FilesToRemove = new List<string>();
        /// <summary>
        /// The files to ignore
        /// </summary>
        protected List<string> FilesToIgnore = new List<string>();
        /// <summary>
        /// The files to remove
        /// </summary>
        protected List<string> CopiedFiles = new List<string>();
        public List<string> Links = new List<string>();
        public List<string> Pictures = new List<string>();
        #endregion

        #region Private Fields
        private static readonly Hashtable HtFilesToRemove = new Hashtable();

        private readonly StringBuilder _notExistingFiles = new StringBuilder();
        private readonly StringBuilder _normalFiles = new StringBuilder();
        private readonly StringBuilder _copiedFiles = new StringBuilder();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkProcessorBase"/> class.
        /// </summary>
        /// <param name="logger">The ILooger instance</param>
        /// <param name="baseDir">The base dir.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="baseEnUsUrl">The base en-us URL.</param>
        /// <param name="baseWoExtUrl">The base URL for en-us links without .md extension.</param>
        /// <param name="externalText">The external text.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="enRepository">The en-US repository path.</param>
        protected LinkProcessorBase(ILogger logger, string baseDir, string baseUrl, string baseEnUsUrl, string baseWoExtUrl, string externalText, string sourceFilePath, string enRepository)
        {
            Logger = logger;
            BaseDir = baseDir;
            BaseUrl = baseUrl;
            BaseEnUsUrl = baseEnUsUrl;
            BaseWoExtUrl = baseWoExtUrl;
            ExternalText = externalText;
            SourceFilePath = sourceFilePath;
            EnRepository = enRepository;
        }
        #endregion

        #region Public Methods        
        /// <summary>
        /// Gets the content of specified log.
        /// </summary>
        /// <returns>The log content</returns>
        public string GetLogContent(LogType logType)
        {
            switch (logType)
            {
                case LogType.NotExistingFiles:
                    return _notExistingFiles.ToString();
                case LogType.NormalFiles:
                    return _normalFiles.ToString();
                case LogType.CopiedFiles:
                    return _copiedFiles.ToString();
                case LogType.ReplacedLinks:
                    return ReplacedLinks.ToString();
                case LogType.ReplacedEnUsLinks:
                    return ReplacedEnUsLinks.ToString();
                case LogType.RemovedFiles:
                    StringBuilder sb = new StringBuilder();
                    foreach (string file in FilesToRemove)
                    {
                        sb.AppendLine(file);
                    }
                    return sb.ToString();
                default:
                    return "";
            }
        }

        /// <summary>
        /// Gets the files which should be removed.
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<string> GetFilesToRemove()
        {
            return FilesToRemove.ToImmutableArray();
        }

        /// <summary>
        /// Gets the files which should be ignored when removing files.
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<string> GetFilesToIgnore()
        {
            return FilesToIgnore.ToImmutableArray();
        }

        /// <summary>
        /// Gets the files which were copied from en-us repository.
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<string> GetCopiedFiles()
        {
            return CopiedFiles.ToImmutableArray();
        }
        #endregion

        #region Public Static methods        
        /// <summary>
        /// Checks if content has "audience: Application User" string.
        /// </summary>
        /// <returns>True of False</returns>
        public static bool ContentHasAudienceApplicationUser(string content)
        {
            if (String.IsNullOrEmpty(content.Trim()))
            {
                return true;
            }

            Regex rgx = new Regex(PatternMeta);
            Match match = rgx.Match(content);
            if (match.Groups.Count == 4)
            {
                string meta = match.Groups[2].Value;
                rgx = new Regex(PatternAudience, RegexOptions.IgnoreCase);
                bool result = rgx.IsMatch(meta);

                if (!result)
                {
                    rgx = new Regex(PatternRedirect, RegexOptions.IgnoreCase);
                    return rgx.IsMatch(meta);
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Processes the content links.
        /// </summary>
        /// <returns></returns>
        public abstract bool ProcessContentLinks();
        /// <summary>
        /// Replaces the link.
        /// </summary>
        /// <returns></returns>
        protected abstract bool ReplaceLink(ref IProcessLinkParameter parameters, LinkType linkType);
        #endregion

        #region Protected Methods

        /// <summary>
        /// Processes the link.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="rawLink">The raw link.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        protected bool ProcessLink(string href, string rawLink, IProcessLinkParameter parameters)
        {
            if (TryGetAbsoluteLinkOnDisk(BaseDir, href, out string path))
            {
                if (File.Exists(path))
                {
                    string extension = Path.GetExtension(path);
                    if (extension != null && extension.Equals(".MD", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (NeedsToBeRemoved(path))
                        {
                            if (ReplaceLink(ref parameters, LinkType.GeneralLink))
                            {
                                HasModified = true;
                            }
                        }
                        else
                        {
                            _normalFiles.AppendLine($"{href}");
                        }
                    }
                }
                else
                {
                    string extension = Path.GetExtension(path);
                    if (String.IsNullOrEmpty(extension) && (rawLink.StartsWith("\\") || rawLink.StartsWith("/")))
                    {
                        if (ReplaceLink(ref parameters, LinkType.RelativeWoExt))
                        {
                            HasModified = true;
                        }
                    }
                    else
                    {
                        if (EnRepository != null)
                        {
                            if (TryGetAbsoluteLinkOnDisk(EnRepository, href, out string enPath))
                            {
                                if (File.Exists(enPath))
                                {
                                    if (extension != null && extension.Equals(".MD", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (ReplaceLink(ref parameters, LinkType.EnUsLink))
                                        {
                                            HasModified = true;
                                        }
                                    }
                                    else
                                    {
                                        string dir = Path.GetDirectoryName(path);
                                        if (dir != null && !Directory.Exists(dir))
                                        {
                                            Directory.CreateDirectory(dir);
                                        }
                                        File.Copy(enPath, path);
                                        _copiedFiles.AppendLine($"{href}");
                                        CopiedFiles.Add(href);
                                    }
                                }
                                else
                                {
                                    Logger.LogWarning($"The file {href} doesn't exist in the both repositories, link inside {SourceFilePath}");
                                    _notExistingFiles.AppendLine($"{href};{SourceFilePath}");
                                }
                            }
                            else
                            {
                                Logger.LogWarning($"Could not get absolute path for {BaseDir} and {href}");
                            }
                        }
                        else
                        {
                            Logger.LogWarning($"The file {href} doesn't exist, link inside {SourceFilePath}");
                            _notExistingFiles.AppendLine($"{href};{SourceFilePath}");
                        }
                    }
                }
            }
            else
            {
                Logger.LogWarning($"Could not get absolute path for {BaseDir} and {href}");
            }
            return HasModified;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks if file needs to be removed (if it doesn't contain 'audience: Application User').
        /// </summary>
        /// <returns>True or False</returns>
        private bool NeedsToBeRemoved(string file)
        {
            if (HtFilesToRemove.ContainsKey(file))
            {
                return (bool)HtFilesToRemove[file];
            }

            if (File.Exists(file))
            {
                string conceptual = File.ReadAllText(file).Trim();
                bool result = !ContentHasAudienceApplicationUser(conceptual);
                HtFilesToRemove[file] = result;
                return result;
            }
            Logger.LogError($"File {file} doesn't exist!");
            return false;
        }

        /// <summary>
        /// Tries the get absolute link on disk.
        /// </summary>
        /// <param name="baseDir">The base dir.</param>
        /// <param name="file">The file.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private bool TryGetAbsoluteLinkOnDisk(string baseDir, string file, out string path)
        {
            path = null;
            if (file.StartsWith("http"))
            {
                return false;
            }

            string clearLink = file.TrimStart('~').TrimStart('\\').TrimStart('/');
            int indexOfQuery = clearLink.IndexOf("?", StringComparison.Ordinal);
            if (indexOfQuery > -1)
            {
                clearLink = clearLink.Substring(0, indexOfQuery);
            }
            int indexOfHash = clearLink.IndexOf("#", StringComparison.Ordinal);
            if (indexOfHash > -1)
            {
                clearLink = clearLink.Substring(0, indexOfHash);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(baseDir, clearLink));
            path = dirInfo.FullName;
            return true;
        }
        #endregion
    }

    /// <summary>
    /// Log Type
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// The not existing files
        /// </summary>
        NotExistingFiles = 1,

        /// <summary>
        /// The normal files
        /// </summary>
        NormalFiles = 2,

        /// <summary>
        /// The copied files
        /// </summary>
        CopiedFiles = 3,

        /// <summary>
        /// The removed files
        /// </summary>
        RemovedFiles = 4,

        /// <summary>
        /// The replaced links
        /// </summary>
        ReplacedLinks = 5,

        /// <summary>
        /// The replaced en-US links
        /// </summary>
        ReplacedEnUsLinks = 6
    }

    /// <summary>
    /// Link type
    /// </summary>
    public enum LinkType
    {
        /// <summary>
        /// The general link
        /// </summary>
        GeneralLink = 1,
        /// <summary>
        /// The link to en-us repo
        /// </summary>
        EnUsLink = 2,
        /// <summary>
        /// The relative link without extension
        /// </summary>
        RelativeWoExt = 3
    }
}

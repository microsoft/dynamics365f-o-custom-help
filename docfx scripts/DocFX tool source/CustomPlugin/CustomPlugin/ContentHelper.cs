namespace CustomPlugin
{
    #region Using
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.DocAsCode.Common;
    #endregion

    /// <summary>
    /// Contains such methods to check/manipulate the text content.
    /// </summary>
    public static class ContentHelper
    {
        #region Constants        
        /// <summary>
        /// The pattern for the meta block
        /// </summary>
        private const string patternMeta = @"(---)([.\s\w\W]*)(---)";
        /// <summary>
        /// The pattern for 'audience: Application User'
        /// </summary>
        private const string patternAudience = @"(audience)\s*?:\s*?.*?(Application User)";
        #endregion

        #region Private Fields
        private static Hashtable _htFilesToRemove = new Hashtable();
        #endregion

        #region Public Methods        
        /// <summary>
        /// Checks if the Content has audience:application user.
        /// </summary>
        /// <param name="conceptual">The conceptual.</param>
        /// <returns></returns>
        public static bool ContentHasAudienceApplicationUser(string conceptual)
        {
            if (String.IsNullOrEmpty(conceptual.Trim()))
            {
                return true;
            }

            Regex rgx = new Regex(patternMeta);
            Match match = rgx.Match(conceptual);
            if (match.Groups.Count == 4)
            {
                string meta = match.Groups[2].Value;
                rgx = new Regex(patternAudience);
                bool result = rgx.IsMatch(meta);
                return result;
            }
            return false;
        }

        /// <summary>
        /// Checks if file needs to be removed (if it doesn't contain 'audience: Application User').
        /// </summary>
        /// <param name="baseDir">The base dir (https://github.com...).</param>
        /// <param name="file">The relative file path</param>
        /// <returns>True or False</returns>
        public static bool NeedsToBeRemoved(string baseDir, string file)
        {
            if (file.StartsWith("http"))
            {
                return false;
            }

            string clearLink = file.TrimStart('~').TrimStart('\\').TrimStart('/');
            int indexOfQuery = clearLink.IndexOf("?");
            if (indexOfQuery > -1)
            {
                clearLink = clearLink.Substring(0, indexOfQuery);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(baseDir, clearLink));
            string path = dirInfo.FullName;

            if (_htFilesToRemove.ContainsKey(path))
            {
                return (bool)_htFilesToRemove[path];
            }
            else
            {
                if (File.Exists(path))
                {
                    using (FileStream stream = File.OpenRead(path))
                    {
                        byte[] buffer = UtilityHelper.ReadFromStream(stream);
                        stream.Close();
                        string conceptual = Encoding.UTF8.GetString(buffer);
                        //string conceptual = File.ReadAllText(path).Trim();
                        bool result = !ContentHasAudienceApplicationUser(conceptual);
                        _htFilesToRemove[path] = result;
                        return result;
                    }
                }
                else
                {
                    Logger.LogWarning($"File {path} does not exist");
                }
                return false;
            }
        }
        #endregion
    }
}

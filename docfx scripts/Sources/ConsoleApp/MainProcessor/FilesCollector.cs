using System;
using System.Collections.Generic;
using System.IO;

namespace MainProcessor
{
    /// <summary>
    /// Finds the files recursively
    /// </summary>
    public class FilesCollector
    {
        private readonly ILogger _logger;
        /// <summary>
        /// The delegate for progress bar.
        /// </summary>
        /// <param name="currenFile">The file name of current file</param>
        /// /// <param name="count">The count of found files</param>
        public delegate void FindFilesHandler(string currenFile, int count);

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesCollector"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public FilesCollector(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Finds all files by mask.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        public string[] FindAllFiles(string path, string mask, FindFilesHandler handler = null)
        {
            List<string> files = new List<string>();
            FindAllFilesRecursively(path, mask, files, handler);
            return files.ToArray();
        }

        /// <summary>
        /// Finds all files recursively.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="list">The list.</param>
        /// <param name="handler">The handler.</param>
        private void FindAllFilesRecursively(string path, string mask, List<string> list, FindFilesHandler handler = null)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(path))
                {
                    foreach (string f in Directory.GetFiles(d, mask))
                    {
                        list.Add(f);
                        handler?.Invoke(f, list.Count);
                    }
                    FindAllFilesRecursively(d, mask, list, handler);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
            }
        }
    }
}

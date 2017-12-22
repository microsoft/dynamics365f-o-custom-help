namespace ConsoleApp.Helpers
{
    #region Using
    using System;
    using System.IO;
    using LibGit2Sharp;
    #endregion

    public class RepoHelper
    {
        #region Static Methods
        /// <summary>
        /// Removes unnecessary folders.
        /// </summary>
        /// <param name="pathToRepo">The path to repo.</param>
        /// <param name="pathToDocFxJson">The path to document fx json.</param>
        public static void CleanRepo(string pathToRepo, string pathToDocFxJson)
        {
            if (!String.IsNullOrEmpty(pathToRepo))
            {
                string gitDirPath = Path.Combine(pathToRepo, ".git");
                if (Directory.Exists(gitDirPath))
                {
                    Directory.Delete(gitDirPath, true);
                }
            }

            string dirDocfx = Path.GetDirectoryName(pathToDocFxJson);
            if (dirDocfx != null)
            {
                string objPath = Path.Combine(dirDocfx, "obj");
                if (Directory.Exists(objPath))
                {
                    Directory.Delete(objPath, true);
                }
            }
        }

        /// <summary>
        /// Checks the cloned repository.
        /// </summary>
        /// <param name="outDir">The out dir.</param>
        /// <param name="jsonPath">The json path.</param>
        /// <param name="path">The result path to docfx.json file.</param>
        /// <returns>True if OK</returns>
        public static bool TryToCheckClonedRepo(string outDir, string jsonPath, out string path)
        {
            path = Path.Combine(Path.Combine(outDir, jsonPath.TrimStart('/')), "docfx.json");
            return File.Exists(path);
        }
        #endregion

        #region Instance        
        /// <summary>
        /// The delegate for progress bar.
        /// </summary>
        /// <param name="percentCompleted">The percent completed.</param>
        public delegate void CloneProgressHandler(int percentCompleted);
        /// <summary>
        /// The event.
        /// </summary>
        private CloneProgressHandler _onProgress;

        /// <summary>
        /// Clones the repository from specified URL to specified destination folder.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="handler">The handler.</param>
        /// <returns></returns>
        public string Clone(string url, string dest, CloneProgressHandler handler)
        {
            _onProgress = handler;
            CloneOptions options = new CloneOptions();
            options.OnTransferProgress += OnTransferProgress;
            return Repository.Clone(url, dest, options);
        }

        /// <summary>
        /// Called when [transfer progress].
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <returns></returns>
        private bool OnTransferProgress(TransferProgress progress)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.CursorVisible = false;
            int percent = progress.ReceivedObjects > 0 ? (progress.ReceivedObjects * 100 / progress.TotalObjects) : 0;
            _onProgress(percent);
            return true;
        }
        #endregion
    }

}

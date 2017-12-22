using LibGit2Sharp;
using System;

namespace MainProcessor
{
    /// <summary>
    /// This class can clone the repository.
    /// </summary>
    public class CloneProcessor
    {
        private readonly ILogger _logger;
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
        /// Initializes a new instance of the <see cref="CloneProcessor"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CloneProcessor(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Tries the clone repository.
        /// </summary>
        /// <param name="repoUrl">The repo URL.</param>
        /// <param name="outDir">The out dir.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="clonedRepoPath">The cloned repo path.</param>
        /// <returns></returns>
        public bool TryCloneRepository(string repoUrl, string outDir, CloneProgressHandler handler, out string clonedRepoPath)
        {
            _onProgress = handler;
            _logger.LogInfo("Cloning repository is in progress. It may take awhile...");

            clonedRepoPath = null;

            CloneOptions options = new CloneOptions();
            options.OnTransferProgress += OnTransferProgress;

            try
            {
                clonedRepoPath = Repository.Clone(repoUrl, outDir, options);
            }
            catch (Exception ex)
            {
                _logger.LogError();
                _logger.LogError($"Error cloning repository: {ex.Message}");
                return false;
            }
            _logger.LogInfo();
            _logger.LogInfo("Cloning repository is completed.");

            return true;
        }

        /// <summary>
        /// Called when [transfer progress].
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <returns></returns>
        private bool OnTransferProgress(TransferProgress progress)
        {
            int percent = progress.ReceivedObjects > 0 ? (progress.ReceivedObjects * 100 / progress.TotalObjects) : 0;
            _onProgress(percent);
            return true;
        }
    }
}

namespace ConsoleApp
{
    #region Using
    using System;
    using System.Linq;
    #endregion

    /// <summary>
    /// Helper for console command line
    /// </summary>
    public static class CommandLineHelper
    {
        /// <summary>
        /// The -repo text
        /// </summary>
        private const string _repoText = "-repo";
        /// <summary>
        /// The -json text
        /// </summary>
        private const string _jsonText = "-json";
        /// <summary>
        /// The -replaceURL text
        /// </summary>
        private const string _replaceUrlText = "-replaceUrl";
        /// <summary>
        /// The -out text
        /// </summary>
        private const string _outText = "-out";
        /// <summary>
        /// The -donotclone text
        /// </summary>
        private const string _donotcloneText = "-donotclone";
        /// <summary>
        /// The -removeGitFolder text
        /// </summary>
        private const string _removeGitFolderText = "-removeGitFolder";
        /// <summary>
        /// The "conceptual log"
        /// </summary>
        private const string _conceptualLogText = "-conceptualLog";
        /// <summary>
        /// The "toc log"
        /// </summary>
        private const string _tocLogText = "-tocLog";
        /// <summary>
        /// The donotshowinfo param
        /// </summary>
        private const string _donotshowinfoText = "-doNotShowInfo";

        /// <summary>
        /// Prints the usage.
        /// </summary>
        public static void PrintUsage()
        {

            ConsoleHelper.WarningWriteLine($"USAGE: app.exe [{_repoText} <URL of repository> | {_donotcloneText}] [{_removeGitFolderText}] {_jsonText} <relative path to dir with docfx.json> {_replaceUrlText} <URL to be replaced> {_outText} <path to output folder> [{_conceptualLogText} <path>] [{_tocLogText} <path>]");
            ConsoleHelper.InfoWriteLine($"{_repoText} - http/https or any other URL of GIT");
            ConsoleHelper.InfoWriteLine($"{_donotcloneText} - tool wont clone the repo");
            ConsoleHelper.InfoWriteLine($"{_removeGitFolderText} - tool will remove the \".git\" directory");
            ConsoleHelper.InfoWriteLine($"{_jsonText} - relative path to directory with docfx.json file");
            ConsoleHelper.InfoWriteLine($"{_replaceUrlText} - base URL which is used for replacing the links");
            ConsoleHelper.InfoWriteLine($"{_outText} - destination folder");
            ConsoleHelper.InfoWriteLine($"{_conceptualLogText} - path to the log for conceptual files (.md)");
            ConsoleHelper.InfoWriteLine($"{_tocLogText} - path to the log for TOC files (toc.*)");
            ConsoleHelper.InfoWriteLine();
            ConsoleHelper.EventWriteLine($"Example: app.exe {_repoText} \"https://github.com/MicrosoftDocs/Dynamics-365-unified-Operations-public\" {_jsonText} articles/ {_replaceUrlText} \"https://docs.microsoft.com/en-us/dynamics365/unified-operations/\" {_outText} \"d:\\myrepository\"");
        }

        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>True if all is OK</returns>
        public static bool ValidateParameters(string[] args)
        {
            return ((args.Any(a => a.Equals(_repoText, StringComparison.InvariantCultureIgnoreCase)) || args.Any(a => a.Equals(_donotcloneText, StringComparison.InvariantCultureIgnoreCase)))
                && args.Any(a => a.Equals(_jsonText, StringComparison.InvariantCultureIgnoreCase))
                && args.Any(a => a.Equals(_replaceUrlText, StringComparison.InvariantCultureIgnoreCase))
                && args.Any(a => a.Equals(_outText, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <summary>
        /// Parses the parameters and return new instance of <see cref="ConsoleParameters"/>
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns><see cref="ConsoleParameters"/> instance</returns>
        public static ConsoleParameters ParseParameters(string[] args)
        {
            string repoParam = null;
            string jsonParam = null;
            string replaceParam = null;
            string outParam = null;
            bool donotcloneParam = false;
            bool removeGitFolderParam = false;
            string conceptualLogParam = null;
            string tocLogParam = null;
            bool donotshowinfoParam = false;

            int indexOfRepoParam = Array.IndexOf(args, _repoText);
            if (indexOfRepoParam > -1)
            {
                repoParam = args[indexOfRepoParam + 1];
            }

            int indexOfJsonParam = Array.IndexOf(args, _jsonText);
            if (indexOfJsonParam > -1)
            {
                jsonParam = args[indexOfJsonParam + 1];
            }

            int indexOfReplaceParam = Array.IndexOf(args, _replaceUrlText);
            if (indexOfReplaceParam > -1)
            {
                replaceParam = args[indexOfReplaceParam + 1];
            }

            int indexOfOutParam = Array.IndexOf(args, _outText);
            if (indexOfOutParam > -1)
            {
                outParam = args[indexOfOutParam + 1];
            }

            donotcloneParam = Array.IndexOf(args, _donotcloneText) > -1;
            removeGitFolderParam = Array.IndexOf(args, _removeGitFolderText) > -1;

            int indexOfConceptualLogParam = Array.IndexOf(args, _conceptualLogText);
            if (indexOfConceptualLogParam > -1)
            {
                conceptualLogParam = args[indexOfConceptualLogParam + 1];
            }

            int indexOfTocLogParam = Array.IndexOf(args, _tocLogText);
            if (indexOfTocLogParam > -1)
            {
                tocLogParam = args[indexOfTocLogParam + 1];
            }

            donotshowinfoParam = Array.IndexOf(args, _donotshowinfoText) > -1;

            return new ConsoleParameters()
            {
                repoUrl = repoParam,
                jsonPath = jsonParam,
                replaceUrl = replaceParam,
                outDir = outParam,
                doNotClone = donotcloneParam,
                removeGitFolder = removeGitFolderParam,
                conceptualLog = conceptualLogParam,
                tocLog = tocLogParam,
                doNotShowInfo = donotshowinfoParam
            };
        }

        /// <summary>
        /// Used for command line parameters.
        /// </summary>
        public sealed class ConsoleParameters
        {
            /// <summary>
            /// The repository URL to be cloned
            /// </summary>
            public string repoUrl;
            /// <summary>
            /// The docfx.json relative path
            /// </summary>
            public string jsonPath;
            /// <summary>
            /// The base URL which is used for replacing the links
            /// </summary>
            public string replaceUrl;
            /// <summary>
            /// The destination directory
            /// </summary>
            public string outDir;
            /// <summary>
            /// The "do not clone" parameter
            /// </summary>
            public bool doNotClone;
            /// <summary>
            /// The "remove git folder" parameter
            /// </summary>
            public bool removeGitFolder;
            /// <summary>
            /// The conceptual log
            /// </summary>
            public string conceptualLog;
            /// <summary>
            /// The toc log
            /// </summary>
            public string tocLog;
            /// <summary>
            /// The "doNotShowInfo" param
            /// </summary>
            public bool doNotShowInfo;
        }
    }
}

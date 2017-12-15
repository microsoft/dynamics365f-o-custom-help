namespace ConsoleApp
{
    #region Using
    using System;
    using System.IO;
    using LibGit2Sharp;
    using static ConsoleApp.CommandLineHelper;
    using System.Diagnostics;
    #endregion

    /// <summary>
    /// This class contains entry point of application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The "custom plugin name"
        /// </summary>
        private const string _customPluginName = "custom";

        #region Entry Point
        /// <summary>
        /// The entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            ConsoleParameters parameters = InitialRun(args);
            if (!parameters.doNotClone)
            {
                CheckFreeSpace(parameters.repoUrl, parameters.outDir);
            }

            string clonedRepoPath = parameters.doNotClone ? parameters.outDir : CloneRepository(parameters);
            //string clonedRepoPath = parameters.doNotClone ? parameters.outDir : @"d:\myrepository_azure\";

            string pathToDocFxJson = CheckClonedRepo(parameters);
            string tempDocFxZipFile = CopyDocFxZip();
            string tempDocFxDir = ExtractDocFxZip(tempDocFxZipFile);
            string pathToCustomPlugin = CopyCustomPlugin(pathToDocFxJson);
            string pathToTemplateZip = CopyTemplate();
            string templateTempDir = ExtractTemplateZip(pathToTemplateZip);
            UtilityHelper.CopyDirectory(templateTempDir, Path.GetDirectoryName(pathToDocFxJson));
            ModifyDocFxJsonFile(pathToDocFxJson, parameters.replaceUrl, _customPluginName, parameters.conceptualLog, parameters.tocLog);

            string docfxexe = Path.Combine(tempDocFxDir, "docfx.exe");
            int exitCode = RunDocFX(docfxexe, pathToDocFxJson, parameters.doNotShowInfo);
            if (exitCode != 0)
            {
                ConsoleHelper.ErrorWrite($"ERROR: exit code {exitCode}");
            }

            RevertDocFxJson(pathToDocFxJson, _customPluginName);
            CleanRepo(parameters.removeGitFolder ? clonedRepoPath : null, pathToDocFxJson);
            RemoveTemp(new string[] { tempDocFxDir, tempDocFxZipFile, pathToTemplateZip, templateTempDir, Path.Combine(Path.GetDirectoryName(pathToDocFxJson), _customPluginName) });

            Exit();
        }

        /// <summary>
        /// Checks the free space.
        /// </summary>
        /// <param name="repoUrl">The repo URL.</param>
        /// <param name="outDir">The out dir.</param>
        private static void CheckFreeSpace(string repoUrl, string outDir)
        {
            ConsoleHelper.InfoWrite("Checking free space...");
            int size = UtilityHelper.GetSizeOfRepo(repoUrl);
            string tempPath = Path.GetTempPath();
            DriveInfo system = new DriveInfo(Path.GetPathRoot(tempPath));
            if (size > system.AvailableFreeSpace / 1024)
            {
                ConsoleHelper.ErrorWrite("ERROR");
                ConsoleHelper.ErrorWriteLine($"Not enought free space on {Path.GetPathRoot(tempPath)}. Please allocate at least {(size / 1024) + Properties.Resources.docfx.Length / 1024 / 1024 + 10}Mb.");
                Exit(1);
            }

            DriveInfo dest = new DriveInfo(Directory.GetDirectoryRoot(outDir));
            if (size > dest.AvailableFreeSpace / 1024)
            {
                ConsoleHelper.ErrorWrite("ERROR");
                ConsoleHelper.ErrorWriteLine($"Not enought free space on {Path.GetPathRoot(outDir)}. Please allocate at least {(size / 1024) + Properties.Resources.docfx.Length / 1024 / 1024 + 10}Mb.");
                Exit(1);
            }
            ConsoleHelper.EventWriteLine("COMPLETED");
        }

        /// <summary>
        /// Copies the custom plugin library.
        /// </summary>
        /// <param name="pathToDocFxJson">The path to docfx.json file.</param>
        /// <returns></returns>
        private static string CopyCustomPlugin(string pathToDocFxJson)
        {
            string rootPath = Path.GetDirectoryName(pathToDocFxJson);
            string pluginPath = Path.Combine(rootPath, _customPluginName);
            if (Directory.Exists(pluginPath))
            {
                Directory.Delete(pluginPath, true);
            }

            Directory.CreateDirectory(Path.Combine(pluginPath, "Plugins"));
            string dllPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "CustomPlugin.dll");

            File.Copy(dllPath, Path.Combine(pluginPath, "Plugins/CustomPlugin.dll"));
            return pluginPath;

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Processes some pre requirements before program runs.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns><see cref="ConsoleParameters"/> object</returns>
        private static ConsoleParameters InitialRun(string[] args)
        {
            ConsoleHelper.SaveDefaultColor();

            if (!CommandLineHelper.ValidateParameters(args))
            {
                CommandLineHelper.PrintUsage();
                Exit(1);
            }

            ConsoleParameters parameters = CommandLineHelper.ParseParameters(args);
            if (parameters.doNotClone && parameters.repoUrl != null)
            {
                ConsoleHelper.ErrorWriteLine($"Conflict with parameters \"-repo\" and \"-donotclone\".");
                Exit(2);
            }

            if (!parameters.doNotClone && !Uri.TryCreate(parameters.repoUrl, UriKind.Absolute, out Uri uriOfRepo))
            {
                ConsoleHelper.ErrorWriteLine($"Invalid URL of repository: {parameters.repoUrl}");
                Exit(2);
            }

            if (Directory.Exists(parameters.outDir))
            {
                ConsoleHelper.ErrorWriteLine($"The directory {parameters.outDir} is already exist.");
                //Exit(2);
            }

            if (!String.IsNullOrEmpty(parameters.conceptualLog))
            {
                string path = Path.GetDirectoryName(parameters.conceptualLog);
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.ErrorWriteLine($"Could not create directory {path}: {ex.Message}");
                        Exit(1);
                    }
                }
            }

            if (!String.IsNullOrEmpty(parameters.tocLog))
            {
                string path = Path.GetDirectoryName(parameters.tocLog);
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.ErrorWriteLine($"Could not create directory {path}: {ex.Message}");
                        Exit(1);
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Clones the repository.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The path terminated with ".git" to the cloned repository</returns>
        private static string CloneRepository(ConsoleParameters parameters)
        {
            bool cursorVisible = Console.CursorVisible;
            ConsoleHelper.InfoWriteLine("Cloning repository is in progress. It may take awhile...");
            CloneOptions options = new CloneOptions();
            options.OnTransferProgress += OnTransferProgress;

            string clonedRepoPath = null;
            try
            {
                clonedRepoPath = Repository.Clone(parameters.repoUrl, parameters.outDir, options);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorWriteLine();
                ConsoleHelper.ErrorWriteLine($"Error cloning repository: {ex.Message}");
                Exit(2);
            }
            ConsoleHelper.ErrorWriteLine();
            ConsoleHelper.InfoWriteLine("Cloning repository is completed.");
            Console.CursorVisible = cursorVisible;
            return clonedRepoPath;
        }

        private static void CleanRepo(string pathToRepo, string pathToDocFxJson)
        {
            if (!String.IsNullOrEmpty(pathToRepo))
            {
                string gitDirPath = Path.Combine(pathToRepo, ".git");
                if (Directory.Exists(gitDirPath))
                {
                    Directory.Delete(gitDirPath, true);
                }
            }

            string objPath = Path.Combine(Path.GetDirectoryName(pathToDocFxJson), "obj");
            if (Directory.Exists(objPath))
            {
                Directory.Delete(objPath, true);
            }
        }

        /// <summary>
        /// Checks the cloned repository.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The path to docfx.json file</returns>
        private static string CheckClonedRepo(ConsoleParameters parameters)
        {
            string pathToDocFxJson = Path.Combine(Path.Combine(parameters.outDir, parameters.jsonPath.TrimStart('/')), "docfx.json");
            if (!File.Exists(pathToDocFxJson))
            {
                ConsoleHelper.ErrorWriteLine($"ERROR: File {pathToDocFxJson} does not exist");
                Exit(2);
            }
            return pathToDocFxJson;
        }

        /// <summary>
        /// Copies the docfx.zip file to the temporary folder.
        /// </summary>
        /// <returns>The path to docfx.zip file</returns>
        private static string CopyDocFxZip()
        {
            string tempZipFile = null;
            ConsoleHelper.InfoWrite("Copying docfx.zip...");
            try
            {
                tempZipFile = UtilityHelper.SaveToTempFile(Properties.Resources.docfx);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorWrite("ERROR");
                ConsoleHelper.ErrorWriteLine(ex.Message);
                Exit(2);
            }
            ConsoleHelper.EventWriteLine("COMPLETED");
            return tempZipFile;
        }

        /// <summary>
        /// Copies the template.zip and return the path to that file.
        /// </summary>
        /// <returns>The path</returns>
        private static string CopyTemplate()
        {
            string tempZipFile = null;
            ConsoleHelper.InfoWrite("Copying mstemplate.zip...");
            try
            {
                tempZipFile = UtilityHelper.SaveToTempFile(Properties.Resources.mstemplate);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorWrite("ERROR");
                ConsoleHelper.ErrorWriteLine(ex.Message);
                Exit(2);
            }
            ConsoleHelper.EventWriteLine("COMPLETED");
            return tempZipFile;
        }

        /// <summary>
        /// Extracts the docfx.zip to the temporary directory.
        /// </summary>
        /// <param name="tempZipFile">The temporary zip file.</param>
        /// <returns>Temporary directory path with unzipped files</returns>
        private static string ExtractDocFxZip(string tempZipFile)
        {
            string tempDir = null;
            ConsoleHelper.InfoWrite("Extracting docfx.zip...");
            try
            {
                tempDir = UtilityHelper.ExtractZip(tempZipFile);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorWrite("ERROR");
                ConsoleHelper.ErrorWriteLine(ex.Message);
                Exit(2);
            }
            ConsoleHelper.EventWriteLine("COMPLETED");
            return tempDir;
        }

        /// <summary>
        /// Extracts the template.zip to the temporary directory.
        /// </summary>
        /// <param name="tempZipFile">The temporary zip file.</param>
        /// <returns>Temporary directory path with unzipped files</returns>
        private static string ExtractTemplateZip(string tempZipFile)
        {
            string tempDir = null;
            ConsoleHelper.InfoWrite("Extracting template.zip...");
            try
            {
                tempDir = UtilityHelper.ExtractZip(tempZipFile);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorWrite("ERROR");
                ConsoleHelper.ErrorWriteLine(ex.Message);
                Exit(2);
            }
            ConsoleHelper.EventWriteLine("COMPLETED");
            return tempDir;
        }

        /// <summary>
        /// Modifies the docfx.json file.
        /// </summary>
        /// <param name="pathToDocFxJson">The path to document fx json.</param>
        /// <param name="replaceUrl">The replace URL.</param>
        private static void ModifyDocFxJsonFile(string pathToDocFxJson, string replaceUrl, string pathToCustomPlugin, string conceptualLog, string tocLog)
        {
            ConsoleHelper.InfoWrite("Modifying docfx.json...");
            try
            {
                UtilityHelper.ModifyDocfxJson(pathToDocFxJson, replaceUrl, pathToCustomPlugin, conceptualLog, tocLog);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorWriteLine("ERROR");
                ConsoleHelper.ErrorWriteLine(ex.Message);
                Exit(2);
            }
            ConsoleHelper.EventWriteLine("COMPLETED");
        }

        private static void RevertDocFxJson(string pathToDocFxJson, string pathToCustomPlugin)
        {
            ConsoleHelper.InfoWrite("Modifying docfx.json...");
            try
            {
                UtilityHelper.RevertDocfxJson(pathToDocFxJson, pathToCustomPlugin);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorWriteLine("ERROR");
                ConsoleHelper.ErrorWriteLine(ex.Message);
                Exit(2);
            }
            ConsoleHelper.EventWriteLine("COMPLETED");
        }

        private static int RunDocFX(string pathToDocFxExe, string pathToDocFxJson, bool doNotShowInfo)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = pathToDocFxJson,
                FileName = pathToDocFxExe,
                WindowStyle = ProcessWindowStyle.Normal,
                //startInfo.CreateNoWindow = true;
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (Process proc = new Process())
            {
                proc.StartInfo = startInfo;

                proc.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                proc.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

                proc.Start();

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                
                proc.WaitForExit();
                return proc.ExitCode;
            }
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data) && outLine.Data.IndexOf("Info:") == -1)
            {
                Console.WriteLine(outLine.Data);
            }
        }

        public class MyEventArgs : EventArgs
        {
            public int X { get; set; }
        }

        /// <summary>
        /// Removes the temporary directory and file.
        /// </summary>
        /// <param name="temps">The temporary dirs and files.</param>
        private static void RemoveTemp(string[] temps)
        {
            ConsoleHelper.InfoWrite("Removing temporary files...");

            foreach (string path in temps)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelper.ErrorWriteLine($"ERROR: {ex.Message}");
                }
            }
            ConsoleHelper.EventWriteLine("COMPLETED");
        }

        /// <summary>
        /// Terminates application with specified exit code.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        private static void Exit(int exitCode = 0)
        {
            ConsoleHelper.RestoreDefaultColor();
            Environment.Exit(exitCode);
        }
        #endregion

        #region Handlers
        /// <summary>
        /// Called when [transfer progress].
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <returns></returns>
        private static bool OnTransferProgress(TransferProgress progress)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.CursorVisible = false;
            int percent = progress.ReceivedObjects > 0 ? (progress.ReceivedObjects * 100 / progress.TotalObjects) : 0;
            Console.Write($"Processing... {percent}% completed.");
            return true;
        }
        #endregion
    }
}

namespace ConsoleApp
{
    #region Using
    using System;
    using System.IO;
    using System.Diagnostics;
    using Helpers;
    using System.Collections.Immutable;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MainProcessor;
    using Newtonsoft.Json.Linq;
    using ConceptualLinkProcessor = MainProcessor.ConceptualLinkProcessor;
    using LinkProcessorBase = MainProcessor.LinkProcessorBase;
    using LogType = MainProcessor.LogType;
    using BizArk.Core.CmdLine;
    #endregion

    /// <summary>
    /// This class contains entry point of application.
    /// </summary>
    class Program
    {
        #region Constants
        /// <summary>
        /// "custom"
        /// </summary>
        private const string CustomPluginName = "custom";
        /// <summary>
        /// "docfx.zip"
        /// </summary>
        private const string DocFxZip = "docfx.zip";
        /// <summary>
        /// "mstemplate.zip"
        /// </summary>
        private const string MsTemplateZip = "mstemplate.zip";
        /// <summary>
        /// "mstemplate"
        /// </summary>
        private const string MsTemplate = "mstemplate";
        /// <summary>
        /// "token.json"
        /// </summary>
        private const string TokenJson = "token.json";
        /// <summary>
        /// "inThisArticle"
        /// </summary>
        private const string InThisArticle = "inThisArticle";
        /// <summary>
        /// "docfx.exe"
        /// </summary>
        private const string DocFxExe = "docfx.exe";
        /// <summary>
        /// "ERROR"
        /// </summary>
        private const string Error = "ERROR";
        /// <summary>
        /// "COMPLETED"
        /// </summary>
        private const string Completed = "COMPLETED";
        /// <summary>
        /// "Plugins"
        /// </summary>
        private const string Plugins = "Plugins";
        /// <summary>
        /// "CustomPlugin.dll"
        /// </summary>
        private static readonly string CustomPluginDll = typeof(CustomPlugin.Constants).Assembly.ManifestModule.Name;
        /// <summary>
        /// "Not enought free space on {0}. Please allocate at least {1}Mb."
        /// </summary>
        private const string FreeSpaceError = "Not enought free space on {0}. Please allocate at least {1}Mb.";
        /// <summary>
        /// "Incorrect structure of docfx.json"
        /// </summary>
        private const string DocFxJsonIncorrect = "Incorrect structure of docfx.json";
        #endregion

        private static readonly ILogger Logger = new Logger();
        private static readonly StringBuilder SbNotExistingFiles = new StringBuilder();
        private static readonly StringBuilder SbNormalFiles = new StringBuilder();
        private static readonly StringBuilder SbCopiedFiles = new StringBuilder();
        private static readonly StringBuilder SbReplacedLinks = new StringBuilder();
        private static readonly StringBuilder SbReplacedEnUsLinks = new StringBuilder();
        private static readonly List<string> LRemovedFiles = new List<string>();
        private static readonly List<string> LIgnoredFiles = new List<string>();

        private static bool _isValid = true;
        #region Entry Point

        /// <summary>
        /// The entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            ConsoleHelper.SaveDefaultColor();

            var parameters = new CommandLineArguments();
            parameters.Initialize();

            string[] parameterNames = typeof(CommandLineArguments).GetProperties().Select(p => $"--{p.Name}").ToArray();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ERROR: Please correct the following errors!");
            foreach (string s in args)
            {
                if (s.StartsWith("-") && s[1] != '-' || s.StartsWith("--") && !parameterNames.Any(p => p.Equals(s, StringComparison.InvariantCultureIgnoreCase)))
                {
                    sb.AppendLine($@"Unknown parameter: {s}");
                    _isValid = false;
                }
            }

            if (!_isValid)
            {
                Console.WriteLine(sb.ToString());
            }

            if (!_isValid || !parameters.IsValid())
            {
                Console.WriteLine(parameters.GetHelpText(Console.WindowWidth));
                Console.WriteLine();
                Console.WriteLine(@"Press any key to exit.");
                Console.ReadKey();
                Exit(ExitCodeEnum.InvalidCommandLine);
            }
            ConsoleApplication.RunProgram<CommandLineArguments>(RunMain);
        }

        /// <summary>
        /// The entry point.
        /// </summary>
        /// <param name="parameters"></param>
        private static void RunMain(CommandLineArguments parameters)
        {
            InitialRun(parameters);

            if (!parameters.DoNotClone)
            {
                CheckFreeSpace(parameters.Repo, parameters.Out);
            }

            string clonedRepoPath = Path.Combine(parameters.Out, ".git");
            if (!parameters.DoNotClone)
            {
                clonedRepoPath = CloneRepository(parameters.Repo, parameters.Out);
            }

            string enClonedRepoPath = null;
            if (!String.IsNullOrEmpty(parameters.EnOut))
            {
                enClonedRepoPath = Path.Combine(parameters.EnOut, ".git");
                if (!String.IsNullOrEmpty(parameters.EnRepo))
                {
                    enClonedRepoPath = CloneRepository(parameters.EnRepo, parameters.EnOut);
                }
            }

            if (!RepoHelper.TryToCheckClonedRepo(parameters.Out, parameters.Json, out string pathToDocFxJson))
            {
                Logger.LogError($"{Error}: File {pathToDocFxJson} does not exist");
                Exit(ExitCodeEnum.InvalidRepo);
            }

            string pathToEnDocFxJson = null;
            if (enClonedRepoPath != null && !RepoHelper.TryToCheckClonedRepo(parameters.EnOut, parameters.Json, out pathToEnDocFxJson))
            {
                Logger.LogError($"{Error}: File {pathToEnDocFxJson} does not exist in en-US repository");
                Exit(ExitCodeEnum.InvalidRepo);
            }

            string repoLocalRoot = Path.GetDirectoryName(pathToDocFxJson);
            string repoEnUsLocalRoot = Path.GetDirectoryName(pathToEnDocFxJson);

            if (!DocFxJsonHelper.IsDocFxJsonCorrect(pathToDocFxJson))
            {
                Logger.LogError(DocFxJsonIncorrect);
                Exit(ExitCodeEnum.InvalidRepo);
            }

            FilesCollector filesCollector = new FilesCollector(Logger);
            Logger.LogInfo("Collecting files, it may take awhile...");
            string[] files = filesCollector.FindAllFiles(repoLocalRoot, "*.md", (fname, count) =>
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.CursorVisible = false;
                string file = Path.GetFileName(fname);
                if (file != null && file.Length > 30)
                {
                    file = file.Substring(file.Length - 30, 30);
                }
                Console.Write($@"Found {count} files: {file}");
            });
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new char[80]);
            Console.SetCursorPosition(0, Console.CursorTop);
            Logger.LogInfo($"Found {files.Length} files.");

            string externalText = parameters.ExternalText;
            if (!String.IsNullOrEmpty(externalText))
            {
                externalText = " " + externalText;
            }

            if (!parameters.ReplaceUrl.EndsWith("/"))
            {
                parameters.ReplaceUrl = $"{parameters.ReplaceUrl}/";
            }

            string baseEnUsUrl = parameters.ReplaceUrl;
            string baseWoExtUrl = null;

            if (Uri.TryCreate(parameters.ReplaceUrl, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                baseWoExtUrl = $"{uriResult.Scheme}://{uriResult.Host}";
            }

            foreach (string f in files)
            {
                string content = File.ReadAllText(f);
                StringBuilder newContent = new StringBuilder();
                LinkProcessorBase proc = new ConceptualLinkProcessor(Logger, repoLocalRoot, parameters.ReplaceUrl, baseEnUsUrl, baseWoExtUrl, externalText, repoLocalRoot != null ? f.Replace(repoLocalRoot, "").TrimStart('\\') : "", repoEnUsLocalRoot, content, newContent);
                bool hasModified = proc.ProcessContentLinks();
                if (hasModified)
                {
                    File.WriteAllText(f, newContent.ToString());
                }

                AppendTextLog(LogType.NotExistingFiles, proc.GetLogContent(LogType.NotExistingFiles));
                AppendTextLog(LogType.NormalFiles, proc.GetLogContent(LogType.NormalFiles));
                AppendTextLog(LogType.CopiedFiles, proc.GetLogContent(LogType.CopiedFiles));
                AppendTextLog(LogType.ReplacedLinks, proc.GetLogContent(LogType.ReplacedLinks));
                AppendTextLog(LogType.ReplacedEnUsLinks, proc.GetLogContent(LogType.ReplacedEnUsLinks));
                AppendRemovedFiles(proc.GetFilesToRemove());
                AppendIgnoredFiles(proc.GetFilesToIgnore());

                if (proc.GetCopiedFiles().Length > 0)
                {
                    ProcessCopiedFiles(repoLocalRoot, parameters.ReplaceUrl, baseEnUsUrl, baseWoExtUrl, externalText, repoEnUsLocalRoot, proc.GetCopiedFiles());
                }
            }

            LogFiles logFiles = new LogFiles(parameters.LogsDir);

            StringBuilder sb = new StringBuilder();
            foreach (string file in LRemovedFiles)
            {
                if (!LIgnoredFiles.Contains(file))
                {
                    sb.AppendLine(file);
                    string path = Path.Combine(repoLocalRoot, file);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
            WriteLog(sb.ToString(), logFiles.RemovedFilesLog, "The files which have been removed");

            StringBuilder sbIgnored = new StringBuilder();
            foreach (string file in LIgnoredFiles)
            {
                sbIgnored.AppendLine(file);
            }

            WriteLog(SbNormalFiles.ToString(), logFiles.NormalFilesLog, "The normal files");
            WriteLog(SbNotExistingFiles.ToString(), logFiles.NotExistentFilesLog, "Not Existing Link;Source File");
            WriteLog(SbCopiedFiles.ToString(), logFiles.CopiedFilesLog, "The files which have been copied from en-US repository");
            WriteLog(SbReplacedLinks.ToString(), logFiles.ReplacedLinksLog, "Source file;Link;Title;New Link;New Title");
            WriteLog(SbReplacedEnUsLinks.ToString(), logFiles.ReplacedLanguageLinksLog, "Source file;Link;Title;New Link;New Title");
            //WriteLog(sbIgnored.ToString(), "logs\\ignored files.txt", "The ignored files");

            string tempDocFxZipFile = SaveToTempFile(Properties.Resources.docfx, DocFxZip);
            string tempDocFxDir = ExtractZip(tempDocFxZipFile, DocFxZip);
            CopyCustomPlugin(pathToDocFxJson);
            string pathToTemplateZip = SaveToTempFile(Properties.Resources.mstemplate, MsTemplateZip);
            string templateTempDir = ExtractZip(pathToTemplateZip, MsTemplateZip);
            UtilityHelper.CopyDirectory(templateTempDir, Path.GetDirectoryName(pathToDocFxJson));

            if (String.IsNullOrEmpty(parameters.Lng))
            {
                parameters.Lng = "en-us";
            }
            Logger.LogInfo($"Setting language {parameters.Lng} for template...", false);
            SetLanguage(parameters.Lng, Path.GetDirectoryName(pathToDocFxJson));
            Logger.LogInfo(Completed);
            DocFxJsonHelper.ModifyDocfxJson(pathToDocFxJson, CustomPluginName, parameters.Rtl);

            string docfxexe = Path.Combine(tempDocFxDir, DocFxExe);
            int exitCodeDocFx = RunDocFx(docfxexe, pathToDocFxJson);
            ExitCodeEnum exitCode = ExitCodeEnum.Success;
            if (exitCodeDocFx != 0)
            {
                exitCode = ExitCodeEnum.DocFxError;
                Logger.LogError($"{Error}: exit code {exitCode}");
            }

            DocFxJsonHelper.RevertDocfxJson(pathToDocFxJson, CustomPluginName);
            RepoHelper.CleanRepo(parameters.RemoveGitFolder ? clonedRepoPath : null, pathToDocFxJson);
            string docfxDir = Path.GetDirectoryName(pathToDocFxJson);
            RemoveTemp(new[]
            {
                tempDocFxDir, tempDocFxZipFile, pathToTemplateZip, templateTempDir,
                (docfxDir != null ? Path.Combine(docfxDir, CustomPluginName) : null)
            });

            string consoleOutput = Logger.GetLogContent();
            File.WriteAllText(logFiles.ConsoleLog, consoleOutput);
            Exit(exitCode);
        }

        /// <summary>
        /// Processes the copied files recursively.
        /// </summary>
        /// <param name="baseDir">The base dir.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <param name="baseEnUsUrl">The base en-us URL.</param>
        /// <param name="baseWoExtUrl">The base URL for the en-us links without .md extension.</param>
        /// <param name="externalText">The external text.</param>
        /// <param name="enRepository">The en repository.</param>
        /// <param name="copiedFiles">The copied files.</param>
        private static void ProcessCopiedFiles(string baseDir, string baseUrl, string baseEnUsUrl, string baseWoExtUrl, string externalText, string enRepository, ImmutableArray<string> copiedFiles)
        {
            foreach (string file in copiedFiles)
            {
                string filePath = Path.Combine(baseDir, file.TrimStart('~').TrimStart('\\').TrimStart('/'));
                if (!Path.GetExtension(filePath).Equals(".md", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                string content = File.ReadAllText(filePath);
                StringBuilder newContent = new StringBuilder();
                LinkProcessorBase proc = new ConceptualLinkProcessor(Logger, baseDir, baseUrl, baseEnUsUrl, baseWoExtUrl, externalText, file.TrimStart('~').TrimStart('\\').TrimStart('/'), enRepository, content, newContent);
                bool hasModified = proc.ProcessContentLinks();

                if (hasModified)
                {
                    File.WriteAllText(filePath, newContent.ToString());
                }

                AppendTextLog(LogType.NotExistingFiles, proc.GetLogContent(LogType.NotExistingFiles));
                AppendTextLog(LogType.NormalFiles, proc.GetLogContent(LogType.NormalFiles));
                AppendTextLog(LogType.CopiedFiles, proc.GetLogContent(LogType.CopiedFiles));
                AppendTextLog(LogType.ReplacedLinks, proc.GetLogContent(LogType.ReplacedLinks));
                AppendTextLog(LogType.ReplacedEnUsLinks, proc.GetLogContent(LogType.ReplacedEnUsLinks));
                AppendRemovedFiles(proc.GetFilesToRemove());
                AppendIgnoredFiles(proc.GetFilesToIgnore());

                if (proc.GetCopiedFiles().Length > 0)
                {
                    ProcessCopiedFiles(baseDir, baseUrl, baseEnUsUrl, baseWoExtUrl, externalText, enRepository, proc.GetCopiedFiles());
                }
            }
        }

        /// <summary>
        /// Clones the repository.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="dest">The dest.</param>
        /// <returns></returns>
        private static string CloneRepository(string url, string dest)
        {
            Logger.LogInfo($"Trying to clone {url}.");
            CloneProcessor cloneProcessor = new CloneProcessor(Logger);
            if (!cloneProcessor.TryCloneRepository(url, dest, percentCompleted =>
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.CursorVisible = false;
                Console.Write($@"Processing... {percentCompleted}% completed.");
            }, out var clonedRepoPath))
            {
                Logger.LogError("Failed to clone repository");
                Exit(ExitCodeEnum.RepoCloneError);
            }
            //Logger.LogInfo("Cloning repository is completed.");
            return clonedRepoPath;
        }

        /// <summary>
        /// Appends the log content.
        /// </summary>
        /// <param name="logType">Type of the log.</param>
        /// <param name="text">The text.</param>
        /// <exception cref="System.ArgumentException"></exception>
        private static void AppendTextLog(LogType logType, string text)
        {
            switch (logType)
            {
                case LogType.NotExistingFiles:
                    SbNotExistingFiles.Append(text);
                    break;
                case LogType.NormalFiles:
                    SbNormalFiles.Append(text);
                    break;
                case LogType.CopiedFiles:
                    SbCopiedFiles.Append(text);
                    break;
                case LogType.ReplacedLinks:
                    SbReplacedLinks.Append(text);
                    break;
                case LogType.ReplacedEnUsLinks:
                    SbReplacedEnUsLinks.Append(text);
                    break;
                case LogType.RemovedFiles:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Appends the removed files to the log.
        /// </summary>
        private static void AppendRemovedFiles(ImmutableArray<string> files)
        {
            LRemovedFiles.AddRange(files.Where(f => !LRemovedFiles.Contains(f)));
        }

        private static void AppendIgnoredFiles(ImmutableArray<string> files)
        {
            LIgnoredFiles.AddRange(files.Where(f => !LIgnoredFiles.Contains(f)));
        }

        /// <summary>
        /// Writes the log content into the file.
        /// </summary>
        /// <param name="logContent">Content of the log.</param>
        /// <param name="logFileName">Name of the log file.</param>
        /// <param name="header">The header.</param>
        private static void WriteLog(string logContent, string logFileName, string header)
        {
            if (!String.IsNullOrEmpty(logFileName))
            {
                string dir = Path.GetDirectoryName(logFileName);
                if (dir != null)
                {
                    try
                    {
                        string dirName = Path.GetDirectoryName(logFileName);
                        if (!String.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
                        {
                            Directory.CreateDirectory(dir);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Could not create directory {dir}: {ex.Message}");
                    }
                    
                    File.WriteAllText(logFileName, header + Environment.NewLine + logContent);
                }
            }
        }

        /// <summary>
        /// Copies specified token.json with provided language, modifies docfx.js file
        /// </summary>
        /// <param name="lng">The LNG.</param>
        /// <param name="dir">The dir.</param>
        private static void SetLanguage(string lng, string dir)
        {
            string pathDocFxJs = Path.Combine(dir, MsTemplate, "styles", "docfx.js");

            string pathTokenJson = Path.Combine(dir, MsTemplate, TokenJson);
            string pathTokenJsonLng = Path.Combine(dir, MsTemplate, $"{TokenJson}.{lng}");

            if (!File.Exists(pathTokenJsonLng))
            {
                Logger.LogError($"Specified token.json for {lng} does not exist");
                Exit(ExitCodeEnum.IoError);
            }

            try
            {
                File.Copy(pathTokenJsonLng, pathTokenJson, true);
                string tokenContent = File.ReadAllText(pathTokenJson);
                JObject o = JObject.Parse(tokenContent);
                if (o[InThisArticle] != null)
                {
                    string inThisArticle = o[InThisArticle].ToString();

                    string content = File.ReadAllText(pathDocFxJs);
                    content = content.Replace(@"{{%inThisArticle%}}", inThisArticle);
                    File.WriteAllText(pathDocFxJs, content);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error setting the language for template: {ex.Message}");
                Exit(ExitCodeEnum.IoError);
            }
        }

        /// <summary>
        /// Checks the free space.
        /// </summary>
        /// <param name="repoUrl">The repo URL.</param>
        /// <param name="outDir">The out dir.</param>
        private static void CheckFreeSpace(string repoUrl, string outDir)
        {
            Logger.LogInfo("Checking free space...", false);
            int ourSize = (Properties.Resources.docfx.Length + Properties.Resources.mstemplate.Length) * 4 / 1024;
            int repoSize = 0;
            try
            {
                repoSize = GithubHelper.GetSizeOfRepo(repoUrl);
            }
            catch (Exception)
            {
                // ignored
            }

            DriveInfo dest = new DriveInfo(Directory.GetDirectoryRoot(outDir));
            if (ourSize + repoSize > dest.AvailableFreeSpace / 1024)
            {
                Logger.LogError(Error, false);
                Logger.LogError(String.Format(FreeSpaceError, Path.GetPathRoot(outDir), (ourSize + repoSize) / 1024));
                Exit(ExitCodeEnum.Error);
            }

            string tempPath = Path.GetTempPath();
            DriveInfo system = new DriveInfo(Path.GetPathRoot(tempPath));
            if (ourSize > system.AvailableFreeSpace / 1024)
            {
                Logger.LogError(Error, false);
                Logger.LogError(String.Format(FreeSpaceError, Path.GetPathRoot(tempPath), (ourSize / 1024)));
                Exit(ExitCodeEnum.Error);
            }

            Logger.LogInfo(Completed);
        }

        /// <summary>
        /// Copies the custom plugin library.
        /// </summary>
        /// <param name="pathToDocFxJson">The path to docfx.json file.</param>
        /// <returns></returns>
        private static void CopyCustomPlugin(string pathToDocFxJson)
        {
            string rootPath = Path.GetDirectoryName(pathToDocFxJson);
            if (rootPath != null)
            {
                string pluginPath = Path.Combine(rootPath, CustomPluginName);
                if (Directory.Exists(pluginPath))
                {
                    Directory.Delete(pluginPath, true);
                }

                Directory.CreateDirectory(Path.Combine(pluginPath, Plugins));
                string loc = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                if (loc != null)
                {
                    string dllPath = Path.Combine(loc, CustomPluginDll);

                    File.Copy(dllPath, Path.Combine(pluginPath, $"{Plugins}/{CustomPluginDll}"));
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Processes some pre requirements before program runs.
        /// </summary>
        /// <param name="options">The arguments.</param>
        private static void InitialRun(CommandLineArguments options)
        {
            ConsoleHelper.SaveDefaultColor();

            if (options.DoNotClone && options.Repo != null)
            {
                Logger.LogError("Conflict with parameters \"-repo\" and \"-donotclone\".");
                Exit(ExitCodeEnum.InvalidCommandLine);
            }

            if (options.DoNotClone && options.EnRepo != null)
            {
                Logger.LogError("Conflict with parameters \"-enRepo\" and \"-donotclone\".");
                Exit(ExitCodeEnum.InvalidCommandLine);
            }

            if (!options.DoNotClone && !Uri.TryCreate(options.Repo, UriKind.Absolute, out Uri _))
            {
                Logger.LogError($"Invalid URL of repository: {options.Repo}");
                Exit(ExitCodeEnum.InvalidCommandLine);
            }

            if (!String.IsNullOrEmpty(options.EnRepo) && String.IsNullOrEmpty(options.EnOut))
            {
                Logger.LogError($"URL of en-US repository specified without destination folder (enOut)!");
                Exit(ExitCodeEnum.InvalidCommandLine);
            }

            if (!options.DoNotClone && !String.IsNullOrEmpty(options.EnRepo) && !Uri.TryCreate(options.EnRepo, UriKind.Absolute, out Uri _))
            {
                Logger.LogError($"Invalid URL of en-US repository: {options.EnRepo}");
                Exit(ExitCodeEnum.InvalidCommandLine);
            }

            if (!String.IsNullOrEmpty(options.Lng))
            {
                if (!Constants.AvailableLanguages.Any(o => o.Equals(options.Lng, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Logger.LogError($"Invalid language provided: {options.Lng}");
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Available languages:");
                    foreach (string lng in Constants.AvailableLanguages)
                    {
                        sb.AppendLine(lng);
                    }
                    Logger.LogError(sb.ToString());
                    Exit(ExitCodeEnum.InvalidCommandLine);
                }
            }

            if (Directory.Exists(options.Out) && !options.DoNotClone)
            {
                Logger.LogError($"The directory {options.Out} is already exist.");
                Exit(ExitCodeEnum.RepoCloneError);
            }

            if (!String.IsNullOrEmpty(options.EnRepo) && !String.IsNullOrEmpty(options.EnOut) && Directory.Exists(options.EnOut))
            {
                Logger.LogError($"The directory {options.EnOut} is already exist.");
                Exit(ExitCodeEnum.RepoCloneError);
            }
        }

        /// <summary>
        /// Saves the buffer to temporary file.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="filename">The filename.</param>
        /// <returns>Temporary file name</returns>
        private static string SaveToTempFile(byte[] data, string filename)
        {
            string tempZipFile = null;
            Logger.LogInfo($"Copying {filename}...", false);
            try
            {
                tempZipFile = UtilityHelper.SaveToTempFile(data);
            }
            catch (Exception ex)
            {
                Logger.LogError(Error, false);
                Logger.LogError(ex.Message);
                Exit(ExitCodeEnum.Error);
            }
            Logger.LogInfo(Completed);
            return tempZipFile;
        }

        private static string ExtractZip(string tempZipFile, string originalFileName)
        {
            string tempDir = null;
            Logger.LogInfo($"Extracting {originalFileName}...", false);
            try
            {
                tempDir = UtilityHelper.ExtractZip(tempZipFile);
            }
            catch (Exception ex)
            {
                Logger.LogError(Error, false);
                Logger.LogError(ex.Message);
                Exit(ExitCodeEnum.Error);
            }
            Logger.LogInfo(Completed);
            return tempDir;
        }

        private static int RunDocFx(string pathToDocFxExe, string pathToDocFxJson)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = "\"" + pathToDocFxJson + "\"",
                FileName = pathToDocFxExe,
                WindowStyle = ProcessWindowStyle.Normal,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using (Process proc = new Process())
            {
                proc.StartInfo = startInfo;

                proc.OutputDataReceived += OutputHandler;
                proc.ErrorDataReceived += OutputHandler;

                proc.Start();

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                
                proc.WaitForExit();
                return proc.ExitCode;
            }
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data) && outLine.Data.IndexOf("Info:", StringComparison.Ordinal) == -1)
            {
                if (outLine.Data.IndexOf("Warning", StringComparison.Ordinal) != -1)
                {
                    Logger.LogWarning(outLine.Data);
                }
                else if (outLine.Data.IndexOf("Error", StringComparison.Ordinal) != -1)
                {
                    Logger.LogError(outLine.Data);
                }
                else
                {
                    Logger.LogInfo(outLine.Data);
                }
            }
        }

        /// <summary>
        /// Removes the temporary directory and file.
        /// </summary>
        /// <param name="temps">The temporary dirs and files.</param>
        private static void RemoveTemp(string[] temps)
        {
            Logger.LogInfo("Removing temporary files...", false);

            foreach (string path in temps)
            {
                if (path == null)
                {
                    continue;
                }

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
                    Logger.LogError($"{Error}: {ex.Message}");
                }
            }
            Logger.LogInfo(Completed);
        }

        /// <summary>
        /// Terminates application with specified exit code.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        private static void Exit(ExitCodeEnum exitCode = ExitCodeEnum.Success)
        {
            ConsoleHelper.RestoreDefaultColor();
            Environment.Exit((int)exitCode);
        }
        #endregion
    }
}

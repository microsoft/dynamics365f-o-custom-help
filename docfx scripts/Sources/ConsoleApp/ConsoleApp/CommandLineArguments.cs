using System;
using System.Collections.Generic;
using System.IO;
using BizArk.Core;
using BizArk.Core.CmdLine;

namespace ConsoleApp
{
    /// <summary>
    /// The command line arguments.
    /// </summary>
    /// <seealso cref="BizArk.Core.CmdLine.CmdLineObject" />
    [CmdLineOptions(ArgumentPrefix = "--")]
    public class CommandLineArguments : CmdLineObject
    {
        /// <summary>
        /// Override this method to perform cmd-line validation. It is recommended to call the base method.
        /// </summary>
        /// <returns></returns>
        protected override string[] Validate()
        {
            List<string> errors = new List<string>();
            errors.Add("Please correct the following errors!");

            if (DoNotClone && !String.IsNullOrEmpty(Repo))
            {
                errors.Add(@"Parameters --donotclone and --repo could not be specified together.");
            }

            if (DoNotClone && !String.IsNullOrEmpty(EnRepo))
            {
                errors.Add(@"Parameters --donotclone and --enRepo could not be specified together.");
            }

            if (!DoNotClone && !String.IsNullOrEmpty(Repo))
            {
                Uri uriResult;
                bool result = Uri.TryCreate(Repo, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                {
                    errors.Add(@"Incorrect --repo parameter. Please specify valid absolute URL.");
                }
            }

            if (String.IsNullOrEmpty(Out))
            {
                errors.Add(@"The --Out parameter is empty. Please specify valid path.");
            }
            else
            {
                if (DoNotClone && String.IsNullOrEmpty(Repo) && !Directory.Exists(Out))
                {
                    errors.Add($@"The path specified in --out doesn't exist: {Out}");
                }
                else if (!DoNotClone && !String.IsNullOrEmpty(Repo) && Directory.Exists(Out))
                {
                    errors.Add($@"The path specified in --out already exists: {Out}");
                }
            }

            if (!DoNotClone && !String.IsNullOrEmpty(EnRepo))
            {
                if (String.IsNullOrEmpty(EnOut))
                {
                    errors.Add(@"The --enOut parameter is empty. Please specify valid path.");
                }

                Uri uriResult;
                bool result = Uri.TryCreate(EnRepo, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                {
                    errors.Add(@"Incorrect --enRepo parameter. Please specify valid absolute URL.");
                }
            }

            if (!String.IsNullOrEmpty(EnOut))
            {
                if (DoNotClone && String.IsNullOrEmpty(EnRepo) && !Directory.Exists(EnOut))
                {
                    errors.Add($@"The path specified in --enOut doesn't exist: {EnOut}");
                }
                else if (!DoNotClone && !String.IsNullOrEmpty(EnRepo) && Directory.Exists(EnOut))
                {
                    errors.Add($@"The path specified in --enOut already exists: {EnOut}");
                }
            }

            if (String.IsNullOrEmpty(Json) || Json != null && !Json.Equals("Articles/", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.Add(@"Please specify 'Articles/' in --json parameter.");
            }

            if (String.IsNullOrEmpty(ReplaceUrl))
            {
                errors.Add(@"Please specify valid URL in --replaceUrl parameter.");
            }
            else
            {
                Uri uriResult;
                bool result = Uri.TryCreate(ReplaceUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                {
                    errors.Add(@"Incorrect --replaceUrl parameter. Please specify valid absolute URL.");
                }
            }

            if (String.IsNullOrEmpty(ExternalText))
            {
                errors.Add(@"Please specify --externalText parameter.");
            }


            if (!String.IsNullOrEmpty(Lng) && !String.IsNullOrEmpty(ReplaceUrl))
            {
                if (ReplaceUrl.IndexOf(Lng, StringComparison.InvariantCultureIgnoreCase) == -1)
                {
                    errors.Add(@"Incorrect --lng code. The --replaceUrl must contain a language identifier that match the value of -–lng value");
                }
            }

            if (!String.IsNullOrEmpty(EnOut) && String.IsNullOrEmpty(Lng))
            {
                errors.Add(@"Please specify --lng parameter. Note: the --replaceUrl must contain a language identifier that match the value of -–lng value");
            }

            ValidateLogParameter("--consoleLog", ConsoleLog, ref errors);
            ValidateLogParameter("--removedFilesLog", RemovedFilesLog, ref errors);
            ValidateLogParameter("--normalFilesLog", NormalFilesLog, ref errors);
            ValidateLogParameter("--notExistFilesLog", NotExistFilesLog, ref errors);
            ValidateLogParameter("--copiedFilesLog", CopiedFilesLog, ref errors);
            ValidateLogParameter("--replacedLinksLog", ReplacedLinksLog, ref errors);
            ValidateLogParameter("--replacedEnUsLinksLog", ReplacedEnUsLinksLog, ref errors);

            if (errors.Count > 1)
            {
                return errors.ToArray();
            }
            return base.Validate();
        }

        /// <summary>
        /// Validates the log parameter.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <param name="errors">The errors.</param>
        private void ValidateLogParameter(string paramName, string paramValue, ref List<string> errors)
        {
            if (String.IsNullOrEmpty(paramValue))
            {
                errors.Add($"Please specify valid path and file name in {paramName} parameter");
            }
            else
            {
                string dirName = Path.GetDirectoryName(paramValue);
                if (!String.IsNullOrEmpty(dirName) && !Directory.Exists(dirName))
                {
                    try
                    {
                        Directory.CreateDirectory(dirName);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($@"Could not created directory {dirName} for the {paramName} parameter: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [do not clone].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [do not clone]; otherwise, <c>false</c>.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "true|false")]
        [System.ComponentModel.Description("Do not clone the repository")]
        public bool DoNotClone { get; set; }

        /// <summary>
        /// Gets or sets the repo.
        /// </summary>
        /// <value>
        /// The repo.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "URL")]
        [System.ComponentModel.Description("URL of repository, e.g. https://github.com/MicrosoftDocs...")]
        public string Repo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [remove git folder].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remove git folder]; otherwise, <c>false</c>.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "true|false")]
        [System.ComponentModel.Description("Removes the \".git\" directory")]
        public bool RemoveGitFolder { get; set; }

        /// <summary>
        /// Gets or sets the json.
        /// </summary>
        /// <value>
        /// The json.
        /// </value>
        [CmdLineArg(Usage = "Articles/", ShowInUsage = DefaultBoolean.True, Required = true)]
        [System.ComponentModel.Description("The relative path (starting from the root of the repository) to docfx.json file. Should be 'Articles/'")]
        public string Json { get; set; }

        /// <summary>
        /// Gets or sets the replace URL.
        /// </summary>
        /// <value>
        /// The replace URL.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "URL")]
        [System.ComponentModel.Description("The base URL which is used for replacing the links")]
        public string ReplaceUrl { get; set; }

        /// <summary>
        /// Gets or sets the out.
        /// </summary>
        /// <value>
        /// The out.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path", Required = true)]
        [System.ComponentModel.Description("The destination folder")]
        public string Out { get; set; }

        /// <summary>
        /// Gets or sets the external text.
        /// </summary>
        /// <value>
        /// The external text.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "text", Required = true)]
        [System.ComponentModel.Description("The external text which will be added to the replaced links")]
        public string ExternalText { get; set; }

        /// <summary>
        /// Gets or sets the console log.
        /// </summary>
        /// <value>
        /// The console log.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path", Required = true)]
        [System.ComponentModel.Description("The general log file, e.g. logs/output.log")]
        public string ConsoleLog { get; set; }

        /// <summary>
        /// Gets or sets the removed files log.
        /// </summary>
        /// <value>
        /// The removed files log.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path", Required = true)]
        [System.ComponentModel.Description("The log with files to remove, e.g. logs/removedFiles.log")]
        public string RemovedFilesLog { get; set; }

        /// <summary>
        /// Gets or sets the normal files log.
        /// </summary>
        /// <value>
        /// The normal files log.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path", Required = true)]
        [System.ComponentModel.Description("The log with files which normal processed, e.g. logs/normalFiles.log")]
        public string NormalFilesLog { get; set; }

        /// <summary>
        /// Gets or sets the not exist files log.
        /// </summary>
        /// <value>
        /// The not exist files log.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path", Required = true)]
        [System.ComponentModel.Description("The log with files which don't exist on disk, e.g. logs/notExistFiles.log")]
        public string NotExistFilesLog { get; set; }

        /// <summary>
        /// Gets or sets the copied files log.
        /// </summary>
        /// <value>
        /// The copied files log.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path", Required = true)]
        [System.ComponentModel.Description("The log with files which were copied from en-US repository, e.g. logs/copiedFiles.log")]
        public string CopiedFilesLog { get; set; }

        /// <summary>
        /// Gets or sets the replaced links log.
        /// </summary>
        /// <value>
        /// The replaced links log.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path", Required = true)]
        [System.ComponentModel.Description("The log with replaced links, e.g. logs/replacedLinks.log")]
        public string ReplacedLinksLog { get; set; }

        /// <summary>
        /// Gets or sets the replaced en us links log.
        /// </summary>
        /// <value>
        /// The replaced en-US links log.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path", Required = true)]
        [System.ComponentModel.Description("The log with replaced links to en-US, e.g. logs/replacedEnUsLinks.log")]
        public string ReplacedEnUsLinksLog { get; set; }

        /// <summary>
        /// Gets or sets the enRepo.
        /// </summary>
        /// <value>
        /// URL of en-US repository, e.g. https://github.com/MicrosoftDocs...
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "URL")]
        [System.ComponentModel.Description("URL of en-US repository, e.g. https://github.com/MicrosoftDocs...")]
        public string EnRepo { get; set; }

        /// <summary>
        /// Gets or sets the enOut.
        /// </summary>
        /// <value>
        /// The en out.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "path")]
        [System.ComponentModel.Description("The destination folder for en-US repository")]
        public string EnOut { get; set; }

        /// <summary>
        /// Gets or sets the LNG.
        /// </summary>
        /// <value>
        /// The LNG.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "language code")]
        [System.ComponentModel.Description("The language code, e.g. en-US")]
        public string Lng { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BizArk.Core;
using BizArk.Core.CmdLine;

namespace HtmlFromRepoGenerator
{
    /// <summary>
    /// The command line arguments.
    /// </summary>
    /// <seealso cref="BizArk.Core.CmdLine.CmdLineObject" />
    [CmdLineOptions(ArgumentPrefix = "--")]
    public class CommandLineArguments : CmdLineObject
    {
        private string _logsDir;

        /// <summary>
        /// Override this method to perform cmd-line validation. It is recommended to call the base method.
        /// </summary>
        /// <returns></returns>
        protected override string[] Validate()
        {
            LinkedList<string> errors = new LinkedList<string>();

            if (DoNotClone && !string.IsNullOrEmpty(Repo))
                errors.AddLast(@"Parameters --donotclone and --repo could not be specified together.");

            //if (DoNotClone && !string.IsNullOrEmpty(EnRepo))
            //    errors.AddLast(@"Parameters --donotclone and --enRepo could not be specified together.");

            if (!DoNotClone && !string.IsNullOrEmpty(Repo))
            {
                bool result = Uri.TryCreate(Repo, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                    errors.AddLast(@"Incorrect --repo parameter. Please specify valid absolute URL.");
            }

            if (string.IsNullOrEmpty(Out))
            {
                errors.AddLast(@"The --Out parameter is empty. Please specify valid path.");
            }
            else
            {
                if (DoNotClone && string.IsNullOrEmpty(Repo) && !Directory.Exists(Out))
                    errors.AddLast($@"The path specified in --out doesn't exist: {Out}");
                else if (!DoNotClone && !string.IsNullOrEmpty(Repo) && Directory.Exists(Out))
                    errors.AddLast($@"The path specified in --out already exists: {Out}");
            }

            //if (!DoNotClone && !string.IsNullOrEmpty(EnRepo))
            //{
            //    if (string.IsNullOrEmpty(EnOut))
            //        errors.AddLast(@"The --enOut parameter is empty. Please specify valid path.");

            //    bool result = Uri.TryCreate(EnRepo, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            //    if (!result)
            //        errors.AddLast(@"Incorrect --enRepo parameter. Please specify valid absolute URL.");
            //}

            //if (!string.IsNullOrEmpty(EnOut))
            //{
            //    if (DoNotClone && string.IsNullOrEmpty(EnRepo) && !Directory.Exists(EnOut))
            //        errors.AddLast($@"The path specified in --enOut doesn't exist: {EnOut}");
            //    else if (!DoNotClone && !string.IsNullOrEmpty(EnRepo) && Directory.Exists(EnOut))
            //        errors.AddLast($@"The path specified in --enOut already exists: {EnOut}");
            //}

            if (string.IsNullOrEmpty(Json) || Json != null && !Json.Equals("Articles/", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddLast(@"Please specify 'Articles/' in --json parameter.");
            }

            if (string.IsNullOrEmpty(ReplaceUrl))
            {
                errors.AddLast(@"Please specify valid URL in --replaceUrl parameter.");
            }
            else
            {
                bool result = Uri.TryCreate(ReplaceUrl, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                    errors.AddLast(@"Incorrect --replaceUrl parameter. Please specify valid absolute URL.");
            }

            if (string.IsNullOrEmpty(ExternalText))
                errors.AddLast(@"Please specify --externalText parameter.");

            //if (!string.IsNullOrEmpty(Lng) && !string.IsNullOrEmpty(ReplaceUrl) && ReplaceUrl.IndexOf(Lng, StringComparison.InvariantCultureIgnoreCase) == -1)
            //    errors.AddLast(@"Incorrect --lng code. The --replaceUrl must contain a language identifier that match the value of -–lng value");

            //if (!string.IsNullOrEmpty(EnOut) && string.IsNullOrEmpty(Lng))
            //    errors.AddLast(@"Please specify --lng parameter. Note: the --replaceUrl must contain a language identifier that match the value of -–lng value");

            ValidateLogParameter("--logsDir", LogsDir, errors);

            if (errors.Count > 0)
            {
                errors.AddFirst("Please correct the following errors!");
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
        private void ValidateLogParameter(string paramName, string paramValue, LinkedList<string> errors)
        {
            if (!Directory.Exists(paramValue))
            {
                try
                {
                    Directory.CreateDirectory(paramValue);
                }
                catch (Exception ex)
                {
                    errors.AddLast($@"Could not create directory {paramValue} for the {paramName} parameter: {ex.Message}");
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
        /// Gets or sets the folder to store logs.
        /// </summary>
        /// <value>
        /// The console log.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = ".\\logs", Required = false)]
        [System.ComponentModel.Description("Directory to store logs")]
        public string LogsDir
        {
            get => string.IsNullOrEmpty(_logsDir) ? "logs" : _logsDir;
            set => _logsDir = value;
        }

        public string EnRepo = null;
        public string EnOut = null;
        public string Lng = null;
        public bool Rtl = false;

/*
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

        /// <summary>
        /// Gets or sets the RTL.
        /// </summary>
        /// <value>
        /// The RTL.
        /// </value>
        [CmdLineArg(ShowInUsage = DefaultBoolean.True, Usage = "Right To Left flag")]
        [System.ComponentModel.Description("The Right To Left")]
        public bool Rtl { get; set; }
*/
    }
}

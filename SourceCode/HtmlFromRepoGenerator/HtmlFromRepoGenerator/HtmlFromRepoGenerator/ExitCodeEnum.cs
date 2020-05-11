namespace HtmlFromRepoGenerator
{
    public enum ExitCodeEnum
    {
        /// <summary>
        /// Success
        /// </summary>
        Success = 0,
        /// <summary>
        /// General error
        /// </summary>
        Error = 1,
        /// <summary>
        /// Invalid command line (bad or incorrect arguments)
        /// </summary>
        InvalidCommandLine = 2,
        /// <summary>
        /// The Input/Output error
        /// </summary>
        IoError = 3,
        /// <summary>
        /// The error occurred during repository clone
        /// </summary>
        RepoCloneError = 4,
        /// <summary>
        /// The repository on disk is incorrect or has invalud structure
        /// </summary>
        InvalidRepo = 5,
        /// <summary>
        /// Unsuccessfull run of docfx.exe utility
        /// </summary>
        DocFxError = 6
    }
}

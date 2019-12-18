namespace HtmlFromRepoGenerator.Exceptions
{
    #region Using
    using System;
    #endregion

    /// <summary>
    /// Custom exception for handling issues with docfx.json file.
    /// </summary>
    /// <seealso cref="System.Exception" />
    class DocFxJsonException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxJsonException"/> class.
        /// </summary>
        public DocFxJsonException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxJsonException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DocFxJsonException(string message) : base(message)
		{
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxJsonException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DocFxJsonException(string message, Exception innerException) : base(message, innerException)
		{
        }
    }
}

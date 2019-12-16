using System.Text;
using MainProcessor;

namespace ConsoleApp
{
    /// <summary>
    /// The logger
    /// </summary>
    /// <seealso cref="MainProcessor.ILogger" />
    public class Logger : ILogger
    {
        /// <summary>
        /// The log
        /// </summary>
        private readonly StringBuilder _log = new StringBuilder();

        /// <summary>
        /// Logs the info message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        public void LogInfo(string message = null, bool newLine = true)
        {
            if (newLine)
            {
                ConsoleHelper.InfoWriteLine(message);
            }
            else
            {
                ConsoleHelper.InfoWrite(message);
            }
            _log.AppendLine($"INFO: {message}");
        }

        /// <summary>
        /// Logs the warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        public void LogWarning(string message = null, bool newLine = true)
        {
            if (newLine)
            {
                ConsoleHelper.WarningWriteLine(message);
            }
            else
            {
                ConsoleHelper.WarningWrite(message);
            }
            _log.AppendLine($"WARNING: {message}");
        }

        /// <summary>
        /// Logs the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        public void LogError(string message = null, bool newLine = true)
        {
            if (newLine)
            {
                ConsoleHelper.ErrorWriteLine(message);
            }
            else
            {
                ConsoleHelper.ErrorWrite(message);
            }
            _log.AppendLine($"ERROR: {message}");
        }

        /// <summary>
        /// Gets the content of the log.
        /// </summary>
        /// <returns></returns>
        public string GetLogContent()
        {
            return _log.ToString();
        }
    }
}

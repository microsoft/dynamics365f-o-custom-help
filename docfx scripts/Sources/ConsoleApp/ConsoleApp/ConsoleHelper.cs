namespace ConsoleApp
{
    #region Using
    using System;
    using System.Text;
    #endregion

    /// <summary>
    /// Helper for console output.
    /// </summary>
    public static class ConsoleHelper
    {
        #region Constants        
        /// <summary>
        /// The information color
        /// </summary>
        private const ConsoleColor InfoColor = ConsoleColor.DarkGray;
        /// <summary>
        /// The event color
        /// </summary>
        private const ConsoleColor EventColor = ConsoleColor.Gray;
        /// <summary>
        /// The warning color
        /// </summary>
        private const ConsoleColor WarningColor = ConsoleColor.Yellow;
        /// <summary>
        /// The error color
        /// </summary>
        private const ConsoleColor ErrorColor = ConsoleColor.Red;
        #endregion

        #region Private Field
        /// <summary>
        /// The saved foreground color for future restoring
        /// </summary>
        private static ConsoleColor _foregroundColor;

        private static StringBuilder _log = new StringBuilder();
        #endregion

        #region Public Methods

        public static string CollectConsoleOutput()
        {
            return _log.ToString();
        }

        /// <summary>
        /// Saves the default color for future restoring
        /// </summary>
        public static void SaveDefaultColor()
        {
            _foregroundColor = Console.ForegroundColor;
        }

        /// <summary>
        /// Restores the default color.
        /// </summary>
        public static void RestoreDefaultColor()
        {
            Console.ForegroundColor = _foregroundColor;
        }

        /// <summary>
        /// Outputs the text as INFO.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void InfoWrite(string message = "")
        {
            Console.ForegroundColor = InfoColor;
            Console.Write(message);
            _log.Append(message);
        }

        /// <summary>
        /// Outputs the text as INFO with the new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void InfoWriteLine(string message = "")
        {
            Console.ForegroundColor = InfoColor;
            Console.WriteLine(message);
            _log.AppendLine(message);
        }

        /// <summary>
        /// Outputs the text as EVENT.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void EventWrite(string message = "")
        {
            Console.ForegroundColor = EventColor;
            Console.Write(message);
            _log.Append(message);
        }

        /// <summary>
        /// Outputs the text as EVENT with the new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void EventWriteLine(string message = "")
        {
            Console.ForegroundColor = EventColor;
            Console.WriteLine(message);
            _log.AppendLine(message);
        }

        /// <summary>
        /// Outputs the text as WARNING.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WarningWrite(string message = "")
        {
            Console.ForegroundColor = WarningColor;
            Console.Write(message);
            _log.Append(message);
        }

        /// <summary>
        /// Outputs the text as WARNING with the new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WarningWriteLine(string message = "")
        {
            Console.ForegroundColor = WarningColor;
            Console.WriteLine(message);
            _log.AppendLine(message);
        }

        /// <summary>
        /// Outputs the text as ERROR.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ErrorWrite(string message = "")
        {
            Console.ForegroundColor = ErrorColor;
            Console.Write(message);
            _log.Append(message);
        }

        /// <summary>
        /// Outputs the text as ERROR with the new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ErrorWriteLine(string message = "")
        {
            Console.ForegroundColor = ErrorColor;
            Console.WriteLine(message);
            _log.AppendLine(message);
        }
        #endregion
    }
}

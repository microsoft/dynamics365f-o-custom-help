namespace ConsoleApp
{
    #region Using
    using System;
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
        private const ConsoleColor _InfoColor = ConsoleColor.DarkGray;
        /// <summary>
        /// The event color
        /// </summary>
        private const ConsoleColor _EventColor = ConsoleColor.Gray;
        /// <summary>
        /// The warning color
        /// </summary>
        private const ConsoleColor _WarningColor = ConsoleColor.Yellow;
        /// <summary>
        /// The error color
        /// </summary>
        private const ConsoleColor _ErrorColor = ConsoleColor.Red;
        #endregion

        #region Private Field
        /// <summary>
        /// The saved foreground color for future restoring
        /// </summary>
        private static ConsoleColor _ForegroundColor;
        #endregion

        #region Public Methods
        /// <summary>
        /// Saves the default color for future restoring
        /// </summary>
        public static void SaveDefaultColor()
        {
            _ForegroundColor = Console.ForegroundColor;
        }

        /// <summary>
        /// Restores the default color.
        /// </summary>
        public static void RestoreDefaultColor()
        {
            Console.ForegroundColor = _ForegroundColor;
        }

        /// <summary>
        /// Outputs the text as INFO.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void InfoWrite(string message = "")
        {
            Console.ForegroundColor = _InfoColor;
            Console.Write(message);
        }

        /// <summary>
        /// Outputs the text as INFO with the new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void InfoWriteLine(string message = "")
        {
            Console.ForegroundColor = _InfoColor;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Outputs the text as EVENT.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void EventWrite(string message = "")
        {
            Console.ForegroundColor = _EventColor;
            Console.Write(message);
        }

        /// <summary>
        /// Outputs the text as EVENT with the new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void EventWriteLine(string message = "")
        {
            Console.ForegroundColor = _EventColor;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Outputs the text as WARNING.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WarningWrite(string message = "")
        {
            Console.ForegroundColor = _WarningColor;
            Console.Write(message);
        }

        /// <summary>
        /// Outputs the text as WARNING with the new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WarningWriteLine(string message = "")
        {
            Console.ForegroundColor = _WarningColor;
            Console.WriteLine(message);
        }

        /// <summary>
        /// Outputs the text as ERROR.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ErrorWrite(string message = "")
        {
            Console.ForegroundColor = _ErrorColor;
            Console.Write(message);
        }

        /// <summary>
        /// Outputs the text as ERROR with the new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ErrorWriteLine(string message = "")
        {
            Console.ForegroundColor = _ErrorColor;
            Console.WriteLine(message);
        }
        #endregion
    }
}

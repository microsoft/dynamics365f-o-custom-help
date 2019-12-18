namespace HtmlFromRepoGenerator
{
    #region Using
    using System.IO;
    #endregion

    /// <summary>
    /// Utility helper.
    /// </summary>
    public static class UtilityHelper
    {
        #region Public Methods
        /// <summary>
        /// Saves the buffer to temporary file.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Path to the temp file</returns>
        public static string SaveToTempFile(byte[] buffer)
        {
            string tempZipFile = Path.GetTempFileName();
            File.WriteAllBytes(tempZipFile, buffer);
            return tempZipFile;
        }

        /// <summary>
        /// Extracts the zip and return the path.
        /// </summary>
        /// <param name="tempZipFile">The temporary zip file.</param>
        /// <returns>The path</returns>
        public static string ExtractZip(string tempZipFile)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            while (Directory.Exists(tempDir) || File.Exists(tempDir))
            {
                tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(tempZipFile, tempDir);
            return tempDir;
        }

        /// <summary>
        /// Copies the directory.
        /// </summary>
        /// <param name="sourceDir">The source dir.</param>
        /// <param name="targetDir">The target dir.</param>
        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string fname = Path.GetFileName(directory);
                if (fname != null)
                {
                    CopyDirectory(directory, Path.Combine(targetDir, fname));
                }
            }
        }
        #endregion
    }
}

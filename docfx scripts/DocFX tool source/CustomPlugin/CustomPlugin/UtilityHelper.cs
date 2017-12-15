namespace CustomPlugin
{
    #region Using
    using System;
    using System.IO;
    #endregion

    /// <summary>
    /// Some helper.
    /// </summary>
    class UtilityHelper
    {
        /// <summary>
        /// Reads from the stream and returns an array of the bytes.
        /// </summary>
        /// <param name="cs">The cs.</param>
        /// <returns></returns>
        public static byte[] ReadFromStream(Stream cs)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    byte[] buffer = new byte[2048];
                    int bytesRead;
                    cs.Position = 0;
                    while ((bytesRead = cs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        stream.Write(buffer, 0, bytesRead);
                    }
                    byte[] result = stream.ToArray();
                    return result;
                }
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }
    }
}

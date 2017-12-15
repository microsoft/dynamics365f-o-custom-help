namespace ConsoleApp
{
    #region Using
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using ConsoleApp.Exceptions;
    using Newtonsoft.Json.Linq;
    #endregion

    /// <summary>
    /// Utility helper.
    /// </summary>
    public static class UtilityHelper
    {
        #region Public Methods
        /// <summary>
        /// Modifies the docfx.json file.
        /// </summary>
        /// <param name="docFxJsonPath">The docfx.json file path.</param>
        /// <param name="baseUrl">The replace URL.</param>
        /// <exception cref="ConsoleApp.Exceptions.DocFxJsonException">Incorrect structure of docfx.json</exception>
        /// <exception cref="DocFxJsonException">Incorrect structure of docfx.json</exception>
        public static void ModifyDocfxJson(string docFxJsonPath, string baseUrl, string pathToCustomPlugin, string conceptualLog, string tocLog)
        {
            string docFxJsonContent = File.ReadAllText(docFxJsonPath);
            JObject docfx = JObject.Parse(docFxJsonContent);
            if (docfx["build"] == null)
            {
                throw new DocFxJsonException("Incorrect structure of docfx.json");
            }

            if (docfx["build"]["globalMetadata"] == null)
            {
                docfx["build"]["globalMetadata"] = new JObject();
            }

            docfx["build"]["globalMetadata"]["baseUrl"] = baseUrl;
            docfx["build"]["globalMetadata"]["_noindex"] = true;
            docfx["build"]["globalMetadata"]["_norobots"] = true;

            if (!String.IsNullOrEmpty(conceptualLog))
            {
                docfx["build"]["globalMetadata"]["conceptualLog"] = conceptualLog;
            }

            if (!String.IsNullOrEmpty(tocLog))
            {
                docfx["build"]["globalMetadata"]["tocLog"] = tocLog;
            }

            if (docfx["build"]["template"] == null)
            {
                docfx["build"]["template"] = new JArray();
                ((JArray)docfx["build"]["template"]).Add("mstemplate");
            }
            else
            {
                if (((JArray)docfx["build"]["template"]).Count == 0)
                {
                    ((JArray)docfx["build"]["template"]).Add("mstemplate");
                }
            }

            bool foundCustom = false;
            for (int i = 0; i < ((JArray)docfx["build"]["template"]).Count; i++)
            {
                JToken t = ((JArray)docfx["build"]["template"])[i];
                if (t.Value<string>().Equals(pathToCustomPlugin, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundCustom = true;
                }
            }

            if (!foundCustom)
            {
                ((JArray)docfx["build"]["template"]).Add(pathToCustomPlugin);
            }
            File.WriteAllText(docFxJsonPath, docfx.ToString());
        }

        /// <summary>
        /// Reverts the docfx json.
        /// </summary>
        /// <param name="docFxJsonPath">The document fx json path.</param>
        /// <param name="pathToCustomPlugin">The path to custom plugin.</param>
        /// <exception cref="DocFxJsonException">Incorrect structure of docfx.json</exception>
        public static void RevertDocfxJson(string docFxJsonPath, string pathToCustomPlugin)
        {
            string docFxJsonContent = File.ReadAllText(docFxJsonPath);
            JObject docfx = JObject.Parse(docFxJsonContent);
            if (docfx["build"] == null)
            {
                throw new DocFxJsonException("Incorrect structure of docfx.json");
            }

            if (docfx["build"]["globalMetadata"] != null)
            {
                if (docfx["build"]["globalMetadata"]["baseUrl"] != null)
                {
                    ((JObject)docfx["build"]["globalMetadata"]).Property("baseUrl").Remove();
                }
                if (docfx["build"]["globalMetadata"]["conceptualLog"] != null)
                {
                    ((JObject)docfx["build"]["globalMetadata"]).Property("conceptualLog").Remove();
                }
                if (docfx["build"]["globalMetadata"]["tocLog"] != null)
                {
                    ((JObject)docfx["build"]["globalMetadata"]).Property("tocLog").Remove();
                }
            }

            if (docfx["build"]["template"] != null)
            {
                bool foundCustom = false;
                JToken t = null;
                for (int i = 0; i < ((JArray)docfx["build"]["template"]).Count; i++)
                {
                    t = ((JArray)docfx["build"]["template"])[i];
                    if (t.Value<string>().Equals(pathToCustomPlugin, StringComparison.InvariantCultureIgnoreCase))
                    {
                        foundCustom = true;
                    }
                }

                if (foundCustom && t != null)
                {
                    ((JArray)docfx["build"]["template"]).Remove(t);
                }
            }
            File.WriteAllText(docFxJsonPath, docfx.ToString());
        }

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
                CopyDirectory(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
        }

        /// <summary>
        /// Gets the size of repo (in Kb) or -1 if error.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Size in Kb or -1 if error</returns>
        public static int GetSizeOfRepo(string url)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            SetAllowUnsafeHeaderParsing20();

            Uri uri = new Uri(url);
            if (!uri.Host.Equals("github.com"))
            {
                return -1;
            }
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent:Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
                string newUrl = String.Format("https://api.github.com/repos/{0}{1}", uri.Segments[1], uri.Segments[2]);
                try
                {
                    string content = client.DownloadString(newUrl);
                    JObject json = JObject.Parse(content);
                    return (int)json["size"];
                }
                catch (Exception)
                {
                }
            }
            return -1;
        }

        #endregion

        #region PrivateMethods
        private static bool SetAllowUnsafeHeaderParsing20()
        {
            //Get the assembly that contains the internal class
            Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                      BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
    }
}

namespace HtmlFromRepoGenerator.Helpers
{
    #region Using
    using System;
    using System.Net;
    using System.Reflection;
    using Newtonsoft.Json.Linq;
    #endregion

    public static class GithubHelper
    {
        /// <summary>
        /// Gets the size of repo.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">Only github.com is supported.</exception>
        public static int GetSizeOfRepo(string url)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            SetAllowUnsafeHeaderParsing20();

            Uri uri = new Uri(url);
            if (!uri.Host.Equals("github.com"))
            {
                throw new NotSupportedException("Only github.com is supported.");
            }
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent:Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
                string newUrl = $"https://api.github.com/repos/{uri.Segments[1]}{uri.Segments[2]}";
                string content = client.DownloadString(newUrl);
                JObject json = JObject.Parse(content);
                return (int)json["size"];
            }
        }

        #region PrivateMethods        
        /// <summary>
        /// Allows unsafe header.
        /// </summary>
        private static void SetAllowUnsafeHeaderParsing20()
        {
            Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    object anInstance = aSettingsType.InvokeMember("Section", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                        }
                    }
                }
            }
        }
        #endregion
    }
}

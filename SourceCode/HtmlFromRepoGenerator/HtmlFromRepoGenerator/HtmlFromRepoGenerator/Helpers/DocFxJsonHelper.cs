using System;
using System.IO;
using HtmlFromRepoGenerator.Exceptions;
using Newtonsoft.Json.Linq;

namespace HtmlFromRepoGenerator.Helpers
{
    public class DocFxJsonHelper
    {
        #region Public Methods

        /// <summary>
        /// Determines whether the docfx.json is correct.
        /// </summary>
        /// <param name="docFxJsonPath">The document fx json path.</param>
        /// <returns>
        ///   <c>true</c> if docfx.json is correct; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDocFxJsonCorrect(string docFxJsonPath)
        {
            string docFxJsonContent = File.ReadAllText(docFxJsonPath);
            JObject docfx = JObject.Parse(docFxJsonContent);
            return docfx["build"] != null;
        }

        /// <summary>
        /// Modifies the docfx.json file.
        /// </summary>
        /// <param name="docFxJsonPath">The docfx.json file path.</param>
        /// <param name="pathToCustomPlugin">The path to the cusom plugin.</param>
        /// <param name="rtl"></param>
        /// <exception cref="HtmlFromRepoGenerator.Exceptions.DocFxJsonException">Incorrect structure of docfx.json</exception>
        /// <exception cref="DocFxJsonException">Incorrect structure of docfx.json</exception>
        public static void ModifyDocfxJson(string docFxJsonPath, string pathToCustomPlugin, bool rtl)
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

            docfx["build"]["globalMetadata"]["_noindex"] = true;
            docfx["build"]["globalMetadata"]["_norobots"] = true;
            docfx["build"]["globalMetadata"]["ms.contentlocale"] = "en-us";

            if (rtl)
            {
                docfx["build"]["globalMetadata"]["_rtl"] = true;
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
                if (docfx["build"]["globalMetadata"]["ms.contentlocale"] != null)
                {
                    ((JObject) docfx["build"]["globalMetadata"]).Property("ms.contentlocale").Remove();
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
        #endregion
    }
}

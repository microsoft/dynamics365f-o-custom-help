namespace CustomPlugin
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.DocAsCode.Build.ConceptualDocuments;
    using Microsoft.DocAsCode.Common;
    using Microsoft.DocAsCode.Plugins;
    using static CustomPlugin.UrlHelper;
    #endregion

    /// <summary>
    /// Custom Conceptual Build class.
    /// </summary>
    /// <seealso cref="Microsoft.DocAsCode.Build.ConceptualDocuments.BuildConceptualDocument" />
    [Export(nameof(CustomConceptualProcessor), typeof(IDocumentBuildStep))]
    public class CustomConceptualBuildStep : BuildConceptualDocument
    {
        #region Private Fields
        /// <summary>
        /// The files which will be removed in Post Build.
        /// </summary>
        private List<string> _FilesToRemove;
        #endregion

        #region IDocumentBuildStep implementation
        /// <summary>
        /// Prebuilds the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <param name="host">The host.</param>
        /// <returns>The list of models which will be built</returns>
        public override IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
        {
            StringBuilder log = new StringBuilder();
            List<FileModel> newModels = new List<FileModel>();

            if (_FilesToRemove == null)
            {
                _FilesToRemove = new List<string>();
            }

            string conceptualLog = null;
            if (((Dictionary<string, object>)models[0].Content).ContainsKey("conceptualLog"))
            {
                conceptualLog = ((Dictionary<string, object>)models[0].Content)["conceptualLog"].ToString();
            }

            bool logEnabled = !String.IsNullOrEmpty(conceptualLog);

            foreach (FileModel model in models)
            {
                if (Path.GetFileNameWithoutExtension(model.FileAndType.File).Equals("toc", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                Dictionary<string, object> content = (Dictionary<string, object>)model.Content;
                if (!content.ContainsKey("baseUrl"))
                {
                    newModels.Add(model);
                    continue;
                }

                string conceptual = content["conceptual"].ToString();
                string baseUrl = content["baseUrl"].ToString();
                bool result = ContentHelper.ContentHasAudienceApplicationUser(conceptual);
                if (result)
                {
                    bool hasModified = false;
                    FoundLink[] links = UrlHelper.FindAllLinks(false, conceptual);
                    foreach (FoundLink link in links.GroupBy(k => k).Select(k => k.Key))
                    {
                        string linkClear = link.Link;

                        if (linkClear.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) || linkClear.EndsWith("toc.md", StringComparison.InvariantCultureIgnoreCase) || linkClear.EndsWith("toc.yml", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        try
                        {
                            string href = UrlHelper.BuildFullUrl("/" + model.File, linkClear);
                            if (ContentHelper.NeedsToBeRemoved(model.BaseDir, href))
                            {
                                if (Uri.TryCreate(new Uri(baseUrl), href.TrimStart('\\').TrimEnd(".md"), out Uri uri))
                                {
                                    string query = UrlHelper.GetQueryFromLink(linkClear);
                                    string newLink = String.Format("{0}{1}", uri.AbsoluteUri, query);
                                    conceptual = conceptual.Replace(link.FullMatch, link.FullMatch.Replace(link.Link, newLink));
                                    hasModified = true;
                                    if (logEnabled)
                                    {
                                        log.AppendLine(String.Format("{0};{1};{2}", model.FileAndType.FullPath, link.Link, newLink));
                                    }
                                }
                                else
                                {
                                    Logger.LogWarning($"URI {href} could not be created");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning(ex.Message);
                        }
                    }
                    content["conceptual"] = conceptual;
                    newModels.Add(model);

                    if (hasModified)
                    {
                        File.WriteAllText(model.FileAndType.FullPath, conceptual);
                    }
                }
                else
                {
                    _FilesToRemove.Add(model.FileAndType.FullPath);
                }
            }

            if (logEnabled)
            {
                File.WriteAllText(conceptualLog, log.ToString());
            }

            return base.Prebuild(newModels.ToImmutableList(), host);
        }

        /// <summary>
        /// Postbuilds the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <param name="host">The host.</param>
        public override void Postbuild(ImmutableList<FileModel> models, IHostService host)
        {
            base.Postbuild(models, host);

            Logger.LogInfo($"Removing {_FilesToRemove.Count} files...");
            foreach (string file in _FilesToRemove)
            {
                File.Delete(file);
            }
        }
        #endregion
    }
}

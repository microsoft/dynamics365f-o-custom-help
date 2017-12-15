namespace CustomPlugin
{
    #region Using
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.DocAsCode.Build.TableOfContents;
    using Microsoft.DocAsCode.Common;
    using Microsoft.DocAsCode.DataContracts.Common;
    using Microsoft.DocAsCode.Plugins;
    using System.Text;
    using static CustomPlugin.UrlHelper;
    #endregion

    /// <summary>
    /// Custom Toc Build class.
    /// </summary>
    /// <seealso cref="Microsoft.DocAsCode.Build.TableOfContents.BuildTocDocument" />
    /// <seealso cref="Microsoft.DocAsCode.Plugins.IDocumentBuildStep" />
    [Export(nameof(CustomTocProcessor), typeof(IDocumentBuildStep))]
    class CustomTocBuildStep : BuildTocDocument, IDocumentBuildStep
    {
        #region Constants        

        #endregion

        #region Public Properties        
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => nameof(CustomTocBuildStep);
        #endregion

        #region IDocumentBuildStep implementation                
        /// <summary>
        /// Prebuilds the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <param name="host">The host.</param>
        /// <returns></returns>
        public override IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
        {
            IEnumerable<FileModel> newModels = base.Prebuild(models, host);

            foreach (FileModel model in newModels)
            {
                TocItemViewModel view = (TocItemViewModel)model.Content;
                if (view.Items != null && view.Metadata.ContainsKey("baseUrl"))
                {
                    LoopViaToc(view.Items, model.BaseDir.Replace("/", "\\"), host, view.Metadata["baseUrl"].ToString());
                }
            }
            return newModels;
        }

        /// <summary>
        /// Postbuilds the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <param name="host">The host.</param>
        public override void Postbuild(ImmutableList<FileModel> models, IHostService host)
        {
            base.Postbuild(models, host);

            StringBuilder log = new StringBuilder();
            Hashtable htHref = new Hashtable();

            if (models.Count == 0 || !((TocItemViewModel)models[0].Content).Metadata.ContainsKey("baseUrl"))
            {
                return;
            }

            string tocLog = null;
            if (((TocItemViewModel)models[0].Content).Metadata.ContainsKey("tocLog"))
            {
                tocLog = ((TocItemViewModel)models[0].Content).Metadata["tocLog"].ToString();
            }
            bool logEnabled = !String.IsNullOrEmpty(tocLog);

            TocItemViewModel modelContent = (TocItemViewModel)models[0].Content;
            string baseDir = models[0].BaseDir;
            string baseUrl = modelContent.Metadata["baseUrl"].ToString();

            int filesTotal = 0;
            int filesToRemove = 0;

            foreach (CustomTocFileInfo file in CustomTocProcessor.ListOfFiles)
            {
                string content = File.ReadAllText(file.FullPath);
                FoundLink[] links = UrlHelper.FindAllLinks(Path.GetExtension(file.FullPath).Equals(".yml"), content);
                bool hasModified = false;

                foreach (FoundLink link in links)
                {
                    string linkClear = link.Link;

                    if (linkClear.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) || linkClear.EndsWith("toc.md", StringComparison.InvariantCultureIgnoreCase) || linkClear.EndsWith("toc.yml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    filesTotal++;
                    string href = UrlHelper.BuildFullUrl("/" + file.RelativePath, linkClear);

                    bool? verifyed = null;
                    if (htHref.ContainsKey(href))
                    {
                        verifyed = (bool)htHref[href];
                    }

                    if (verifyed == null)
                    {
                        verifyed = ContentHelper.NeedsToBeRemoved(Path.GetDirectoryName(file.FullPath), linkClear);
                        htHref.Add(href, (bool)verifyed);
                    }

                    if (verifyed == true)
                    {
                        if (Uri.TryCreate(new Uri(baseUrl), href.TrimStart('\\').TrimEnd(".md"), out Uri uri))
                        {
                            string query = UrlHelper.GetQueryFromLink(linkClear);
                            string newLink = String.Format("{0}{1}", uri.AbsoluteUri, query);
                            content = content.Replace(link.FullMatch, link.FullMatch.Replace(link.Link, newLink));
                            hasModified = true;
                            if (logEnabled)
                            {
                                log.AppendLine(String.Format("{0};{1};{2}", file.FullPath, link.Link, newLink));
                            }
                        }
                        else
                        {
                            Logger.LogWarning($"URI {href} could not be created");
                        }
                        filesToRemove++;
                    }
                }

                if (hasModified)
                {
                    File.WriteAllText(file.FullPath, content);
                }
            }

            if (logEnabled)
            {
                File.WriteAllText(tocLog, log.ToString());
            }

            Logger.LogInfo($"Total files: {filesTotal}");
            Logger.LogInfo($"Files to remove: {filesToRemove}");
        }
        #endregion

        #region Private Methods                        
        /// <summary>
        /// Loops via TOC items.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="baseDir">The base dir.</param>
        /// <param name="host">The host.</param>
        /// <param name="baseUrl">The base URL.</param>
        private void LoopViaToc(TocViewModel content, string baseDir, IHostService host, string baseUrl)
        {
            for (int j = 0; j < content.Count; j++)
            {
                TocItemViewModel item = content[j];
                if (item.TopicHref != null)
                {
                    if (ContentHelper.NeedsToBeRemoved(baseDir, item.TopicHref))
                    {
                        if (Uri.TryCreate(new Uri(baseUrl), item.TopicHref.TrimStart('~'), out Uri uri))
                        {
                            item.TopicHref = uri.AbsoluteUri;
                            item.Href = uri.AbsoluteUri;
                        }
                        else
                        {
                            Logger.LogWarning($"URI: {item.OriginalHref}");
                        }
                    }
                }

                if (item.Items != null)
                {
                    LoopViaToc(item.Items, baseDir, host, baseUrl);
                }
            }
        }
        #endregion
    }
}

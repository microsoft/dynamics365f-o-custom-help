namespace CustomPlugin
{
    #region Using
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using Microsoft.DocAsCode.Build.TableOfContents;
    using Microsoft.DocAsCode.Plugins;
    #endregion

    /// <summary>
    /// Custom Toc Processor class.
    /// </summary>
    /// <seealso cref="Microsoft.DocAsCode.Build.TableOfContents.TocDocumentProcessor" />
    [Export(typeof(IDocumentProcessor))]
    public class CustomTocProcessor : TocDocumentProcessor
    {
        #region Private Fields
        /// <summary>
        /// The list of files
        /// </summary>
        private static List<CustomTocFileInfo> _listOfFiles = new List<CustomTocFileInfo>();
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the list of files.
        /// </summary>
        /// <value>
        /// The list of files.
        /// </value>
        public static List<CustomTocFileInfo> ListOfFiles { get => _listOfFiles; }

        /// <summary>
        /// The hashtable
        /// </summary>
        public static Hashtable Hashtable = new Hashtable();
        #endregion

        #region IDocumentProcessor overridden properties
        /// <summary>
        /// Returns the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => nameof(CustomTocProcessor);

        /// <summary>
        /// Gets or sets the build steps.
        /// </summary>
        /// <value>
        /// The build steps.
        /// </value>
        [ImportMany(nameof(CustomTocProcessor))]
        public override IEnumerable<IDocumentBuildStep> BuildSteps { get; set; }
        #endregion

        #region IDocumentProcessor implementation
        public override FileModel Load(FileAndType file, ImmutableDictionary<string, object> metadata)
        {
            FileModel model = base.Load(file, metadata);
            _listOfFiles.Add(new CustomTocFileInfo() { FullPath = model.FileAndType.FullPath, RelativePath = model.LocalPathFromRoot });
            return model;
        }

        /// <summary>
        /// Returns the processing priority.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override ProcessingPriority GetProcessingPriority(FileAndType file)
        {
            if (base.GetProcessingPriority(file) != ProcessingPriority.NotSupported)
            {
                return ProcessingPriority.Highest;
            }

            return ProcessingPriority.NotSupported;
        }
        #endregion
    }
}

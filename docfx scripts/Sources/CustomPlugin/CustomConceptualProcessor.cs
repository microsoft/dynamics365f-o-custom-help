using Microsoft.DocAsCode.Common;

namespace CustomPlugin
{
    #region Using
    using System.Collections.Generic;
    using System.Composition;
    using Microsoft.DocAsCode.Build.ConceptualDocuments;
    using Microsoft.DocAsCode.Plugins;
    #endregion

    /// <summary>
    /// Custom Conceptual Processor class.
    /// </summary>
    /// <seealso cref="Microsoft.DocAsCode.Build.ConceptualDocuments.ConceptualDocumentProcessor" />
    [Export(typeof(IDocumentProcessor))]
    public class CustomConceptualProcessor : ConceptualDocumentProcessor
    {
        #region IDocumentProcessor overridden properties
        /// <summary>
        /// Returns the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => nameof(CustomConceptualProcessor);

        /// <summary>
        /// Gets or sets the build steps.
        /// </summary>
        /// <value>
        /// The build steps.
        /// </value>
        [ImportMany(nameof(CustomConceptualProcessor))]
        public override IEnumerable<IDocumentBuildStep> BuildSteps { get; set; }

        /// <summary>
        /// Returns the incremental context hash.
        /// </summary>
        /// <returns></returns>
        public override string GetIncrementalContextHash() => null;
        #endregion

        #region IDocumentProcessor implementation
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

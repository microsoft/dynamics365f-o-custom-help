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
    #endregion

    /// <summary>
    /// Custom Conceptual Build class.
    /// </summary>
    /// <seealso cref="Microsoft.DocAsCode.Build.ConceptualDocuments.BuildConceptualDocument" />
    [Export(nameof(CustomConceptualProcessor), typeof(IDocumentBuildStep))]
    public class CustomConceptualBuildStep : BuildConceptualDocument
    {
        #region Public Properties        
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => nameof(CustomConceptualBuildStep);
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
            return base.Prebuild(models, host);
        }

        /// <summary>
        /// Postbuilds the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <param name="host">The host.</param>
        public override void Postbuild(ImmutableList<FileModel> models, IHostService host)
        {
            base.Postbuild(models, host);
        }
        #endregion
    }
}

namespace CustomPlugin
{
    #region Using
    using System.Collections.Immutable;
    using System.Composition;
    using Microsoft.DocAsCode.Plugins;
    #endregion

    /// <summary>
    /// Input Metadata Validator class.
    /// </summary>
    /// <seealso cref="Microsoft.DocAsCode.Plugins.IInputMetadataValidator" />
    [Export(typeof(IInputMetadataValidator))]
    class InputMetadataValidator : IInputMetadataValidator
    {
        #region IInputMetadataValidator implementation
        /// <summary>
        /// Validates the specified source file.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="metadata">The metadata.</param>
        /// <exception cref="DocumentException"></exception>
        public void Validate(string sourceFile, ImmutableDictionary<string, object> metadata)
        {
            if (!metadata.ContainsKey("baseUrl"))
            {
                throw new DocumentException($"Metadata 'baseUrl' is not defined. Please define baseUrl in Global Metadata.");
            }
        }
        #endregion
    }
}

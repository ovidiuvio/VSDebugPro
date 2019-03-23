using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace VSDebugCoreLib.Console
{
    internal static class VSDContentTypeDefinition
    {
        public const string ContentType = "vsdscript";

        /// <summary>
        /// Exports the vsd content type
        /// </summary>
        [Export]
        [Name(VSDContentTypeDefinition.ContentType)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition VSDContentType { get; set; }
    }
}
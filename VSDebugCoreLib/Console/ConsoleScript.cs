using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

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

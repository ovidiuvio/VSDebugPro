// Guids.cs
// MUST match guids.h

using System;

namespace VSDebugCoreLib
{
    public static class GuidList
    {
        // Context guids
        public const string GuidVSDebugProPkgString = "BD397717-72D8-460A-B831-0AEB27E057D8";

        public static readonly Guid GuidVSDebugProPkg = new Guid(GuidVSDebugProPkgString);

        // Menu guids
        public static readonly Guid GuidVSDebugProMenuBaseGroup = new Guid("D34760EB-9BE4-49D6-9EB2-D8260FB14AF3");

        public static readonly Guid GuidVSDebugProCmds = new Guid("5E308F9C-8C6C-4EF5-BEAD-422005091961");
        public static readonly Guid GuidVSDebugProConsoleMenu = new Guid("3408ABC4-299F-489A-8203-4165EC9557D4");

        // Command guids
        public static readonly Guid GuidVSDebugProAbout = new Guid("2CB356A8-2154-42AC-B953-4F7E3A5F53F4");

        public static readonly Guid GuidVSDebugProConsole = new Guid("1B349D26-5B87-48DD-A3DB-0CFBA5C4F8E2");
        public static readonly Guid GuidVSDebugProImage = new Guid("5FB955DA-43B9-4E35-9E03-95669472FDD9");
        public static readonly Guid GuidVSDebugProHelp = new Guid("5B184540-B184-4540-A017-B1960519183E");
        public static readonly Guid GuidVSDebugProSettings = new Guid("317DB7DB-D080-42B2-A298-404F42FFD60E");

        public static readonly Guid GuidVSDebugProExploreWD = new Guid("D71DB7A7-6183-4D16-9018-747123D2DD39");
    }
}
﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VSDebugCoreLib {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("VSDebugCoreLib.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to about.
        /// </summary>
        internal static string AboutCommandString {
            get {
                return ResourceManager.GetString("AboutCommandString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to create window!.
        /// </summary>
        internal static string CanNotCreateWindow {
            get {
                return ResourceManager.GetString("CanNotCreateWindow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to VSDebugPro_4-0-0-0
        ///    [+] Added support for VS2022
        ///    [+] Moved to NET 4.8
        ///    [-] Dropped support for older vs versions
        ///    [*] Cleaned up assembly references
        ///    [*] Converted assembly references to package references
        ///
        ///
        ///VSDebugPro_3-4-0-1_2022-05-07
        ///    [+] Always use debugger api
        ///    [+] Memory operations for remote debugging
        ///    [*] Memory ops buffer increased to 64k
        ///    [*] Cleanup
        ///    [*] Updated copyright
        ///
        ///VSDebugPro_3-3-0-1_2019-08-06
        ///    
        ///    [+] The extension is now using Visual [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string changelog {
            get {
                return ResourceManager.GetString("changelog", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Opens the about window..
        /// </summary>
        internal static string CmdAboutDesc {
            get {
                return ResourceManager.GetString("CmdAboutDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Alias allows a more familiar command or name to execute a long string.
        /// </summary>
        internal static string CmdAliasDesc {
            get {
                return ResourceManager.GetString("CmdAliasDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to alias.
        /// </summary>
        internal static string CmdAliasString {
            get {
                return ResourceManager.GetString("CmdAliasString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Memory dump utility..
        /// </summary>
        internal static string CmdDumpMemDesc {
            get {
                return ResourceManager.GetString("CmdDumpMemDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provides help information for commands..
        /// </summary>
        internal static string CmdHelpDesc {
            get {
                return ResourceManager.GetString("CmdHelpDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Opens the debug image visualizer.
        /// </summary>
        internal static string CmdImageDesc {
            get {
                return ResourceManager.GetString("CmdImageDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Load memory utility..
        /// </summary>
        internal static string CmdLoadMemDesc {
            get {
                return ResourceManager.GetString("CmdLoadMemDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to loadmem.
        /// </summary>
        internal static string CmdLoadMemString {
            get {
                return ResourceManager.GetString("CmdLoadMemString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Allocates memory in the process heap..
        /// </summary>
        internal static string CmdMemAllocDesc {
            get {
                return ResourceManager.GetString("CmdMemAllocDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to malloc.
        /// </summary>
        internal static string CmdMemAllocString {
            get {
                return ResourceManager.GetString("CmdMemAllocString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Memory copy utility..
        /// </summary>
        internal static string CmdMemCpyDesc {
            get {
                return ResourceManager.GetString("CmdMemCpyDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to memcpy.
        /// </summary>
        internal static string CmdMemCpyString {
            get {
                return ResourceManager.GetString("CmdMemCpyString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Diff utility..
        /// </summary>
        internal static string CmdMemDiffDesc {
            get {
                return ResourceManager.GetString("CmdMemDiffDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to diff.
        /// </summary>
        internal static string CmdMemDiffString {
            get {
                return ResourceManager.GetString("CmdMemDiffString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Free memory allocated with malloc..
        /// </summary>
        internal static string CmdMemFreeDesc {
            get {
                return ResourceManager.GetString("CmdMemFreeDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to free.
        /// </summary>
        internal static string CmdMemFreeString {
            get {
                return ResourceManager.GetString("CmdMemFreeString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fills a block of memory with a pattern..
        /// </summary>
        internal static string CmdMemSetDesc {
            get {
                return ResourceManager.GetString("CmdMemSetDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to memset.
        /// </summary>
        internal static string CmdMemSetString {
            get {
                return ResourceManager.GetString("CmdMemSetString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Opens product settings dialog..
        /// </summary>
        internal static string CmdSettingsDesc {
            get {
                return ResourceManager.GetString("CmdSettingsDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to settings.
        /// </summary>
        internal static string CmdSettingsString {
            get {
                return ResourceManager.GetString("CmdSettingsString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ovidiu@vsdebug.pro.
        /// </summary>
        internal static string ContactInfo {
            get {
                return ResourceManager.GetString("ContactInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to dumpmem.
        /// </summary>
        internal static string DumpMemString {
            get {
                return ResourceManager.GetString("DumpMemString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to help.
        /// </summary>
        internal static string HelpCmdString {
            get {
                return ResourceManager.GetString("HelpCmdString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to www.vsdebug.pro/support.
        /// </summary>
        internal static string HelpWebsite {
            get {
                return ResourceManager.GetString("HelpWebsite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to image.
        /// </summary>
        internal static string ImageCmdString {
            get {
                return ResourceManager.GetString("ImageCmdString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /**
        /// * VSDebugPro - Visual Studio extension
        /// * 
        /// * 
        /// * MIT License
        /// * Copyright (c) Ovidiu Ionescu
        /// * http://www.vsdebug.pro
        /// *
        /// * Permission is hereby granted, free of charge, to any person obtaining a copy
        /// * of this software and associated documentation files (the &quot;Software&quot;), to deal
        /// * in the Software without restriction, including without limitation the rights
        /// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        /// * copies of the Software, and to permit persons to wh [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string license {
            get {
                return ResourceManager.GetString("license", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to motd.vsdebug.pro.
        /// </summary>
        internal static string MotdUrl {
            get {
                return ResourceManager.GetString("MotdUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to © Copyright Ovidiu Ionescu.
        /// </summary>
        internal static string ProductCopyright {
            get {
                return ResourceManager.GetString("ProductCopyright", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to www.vsdebug.pro.
        /// </summary>
        internal static string Website {
            get {
                return ResourceManager.GetString("Website", resourceCulture);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSDebugCoreLib.Scripting
{
    public abstract class VSDScriptObject
    {
        /// <summary>
        /// script file
        /// </summary>
        protected string _scriptFile;
        /// <summary>
        /// script path
        /// </summary>
        protected string _scriptPath;
        /// <summary>
        /// script context
        /// </summary>
        protected VSDebugContext _context;

        /// <summary>
        /// script object constructor
        /// </summary>
        public VSDScriptObject(VSDebugContext context, string strPath, string strFile)
        {
            _context = context;
            _scriptPath = strPath;
            _scriptFile = strFile;
        }

        /// <summary>
        /// script context interface
        /// </summary>
        public VSDebugContext Context { get => _context; }

        /// <summary>
        /// scripts must implement Initialize interface
        /// </summary>
        public abstract void Initialize();
        /// <summary>
        /// scripts must implement Exit interface
        /// </summary>
        public abstract void Exit();
        /// <summary>
        /// scripts must implement GetMethod interface
        /// </summary>
        public abstract dynamic GetMethod(string strMethod);
        /// <summary>
        /// scripts must implement CallMethod interface
        /// </summary>
        public abstract VSDStatus CallMethod(string strMethod);
    }
}

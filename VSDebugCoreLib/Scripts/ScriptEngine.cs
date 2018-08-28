using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static VSDebugCoreLib.Scripting.VSDScriptApi;

namespace VSDebugCoreLib.Scripting
{
    public class VSDScriptEngine
    {
        private ICollection<VSDScriptObject> _scripts = new List<VSDScriptObject>();

        private VSDebugContext _context;

        public VSDScriptEngine(VSDebugContext context)
        {
            _context = context;
        }

        /// <summary>
        /// expose scripting context
        /// </summary>
        public VSDebugContext Context { get => _context; }

        /// <summary>
        /// Load scripts function
        /// </summary>
        public int LoadScripts(string strPath)
        {
            try
            {
                if (Directory.Exists(strPath))
                {
                    var scriptFiles = Directory.EnumerateFiles(strPath, "*.py");

                    foreach (var script in scriptFiles)
                    {
                        VSDScriptObject obj = new VSDIronScriptObject(_context, strPath, script);
                        obj.Initialize();
                        _scripts.Add(obj);
                    }

                }

                return _scripts.Count;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// ReLoad scripts function
        /// </summary>
        public int ReloadScripts(string strPath)
        {
            try
            {
                // call scripts exit function
                foreach (var script in _scripts)
                {
                    script.Exit();
                }

                // clear scripts list
                _scripts.Clear();

                return LoadScripts(strPath);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}

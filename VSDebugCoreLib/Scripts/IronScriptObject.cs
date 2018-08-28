using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

namespace VSDebugCoreLib.Scripting
{
    public class VSDIronScriptObject : VSDScriptObject
    {
        private ScriptSource    _source;
        private CompiledCode    _code;
        private ScriptEngine    _engine;
        private ScriptScope     _scope;

        public VSDIronScriptObject(VSDebugContext context, string strPath, string strFile)
            :base(context, strPath, strFile)
        {
            try
            {
                // create python environment
                _engine = Python.CreateEngine();
                _source = _engine.CreateScriptSourceFromFile(_scriptFile);
                _code = _source.Compile();

                // set the scope
                _scope = _code.DefaultScope;

                // clear the zipimporter hook to prevent tons of exceptions
                var pc = HostingHelpers.GetLanguageContext(_engine) as PythonContext;
                var hooks = pc.SystemState.Get__dict__()["path_hooks"] as List;
                hooks.Clear();

                // import .NET clr
                _scope.ImportModule("clr");

                // set context
                _scope.SetVariable(VSDScriptApi.strApiSelf, this);

                // execute code into the default scope
                _code.Execute();

            }
            catch (Exception e)
            {
                Context.CONSOLE.Write("Script Exception:" + e.Message);
            }
        }

        public override void Initialize()
        {
            try
            {
                var func = _scope.GetVariable(VSDScriptApi.strApiInit);
                if (null != func)
                    func();
            }
            catch (Exception e)
            {
                Context.CONSOLE.Write("Script Exception:" + e.Message);
            }
        }

        public override void Exit()
        {
            try
            {
                var func = _scope.GetVariable(VSDScriptApi.strApiExit);
                if (null != func)
                    func();
            }
            catch (Exception e)
            {
                Context.CONSOLE.Write("Script Exception:" + e.Message);
            }
        }

        public override dynamic GetMethod(string strMethod)
        {
            try
            {
                var func = _scope.GetVariable(strMethod);
                return func;
            }
            catch (Exception e)
            {
                Context.CONSOLE.Write("Script Exception:" + e.Message);

                return null;
            }
        }

        public override VSDStatus CallMethod(string strMethod)
        {
            try
            {
                var func = _scope.GetVariable(strMethod);
                if (null != func)
                    return func();

                return VSDStatus.VSD_ST_NOT_FOUND;

            }
            catch (Exception e)
            {
                Context.CONSOLE.Write("Script Exception:" + e.Message);

                return VSDStatus.VSD_ST_EXCEPTION;
            }
        }
        
    }
}

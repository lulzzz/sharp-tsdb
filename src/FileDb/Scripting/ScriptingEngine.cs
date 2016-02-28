﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using CustomDbExtensions;
using DbInterfaces.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using Timeenator.Interfaces;

namespace FileDb.Scripting
{
    public class Globals
    {
        public IDb db;
    }

    public class ScriptingEngine
    {
        private IDb _db;
        private string _expression;
        private object _result;
        private Script<IQueryResult> _script;
        private static ScriptOptions _options;
        private static Dictionary<string, Script<IQueryResult>> _scripts = new Dictionary<string, Script<IQueryResult>>();

        public IQueryResult Result => _result as IQueryResult;
        public IObjectQuerySerie ResultAsSerie => _result as IObjectQuerySerie;
        public IObjectQueryTable ResultAsTable => _result as IObjectQueryTable;

        public ScriptingEngine(IDb db, string expression)
        {
            _db = db;
            _expression = expression;
        }

        static ScriptingEngine()
        {
            CreateOptions();
        }

        public static async Task<object> ExecuteTestAsync(string expressionToExecute)
        {
            var script = CSharpScript.Create<object>(expressionToExecute, globalsType: typeof(Globals), options: _options);
            script.Compile();
            return (await script.RunAsync(new Globals())).ReturnValue;
        }


        public ScriptingEngine Execute()
        {
            CreateScript();
            var globals = new Globals()
            {
                db = _db
            };

            ScriptState state = null;

            _script.RunAsync(globals).ContinueWith(s => state = s.Result).Wait();

            _result = state.ReturnValue;

            return this;
        }

        private static void CreateOptions()
        {
            _options = ScriptOptions.Default;
            _options = _options.WithReferences(new[]
            {
                typeof (DateTime).Assembly,
                typeof (IEnumerable<>).Assembly,
                typeof (Enumerable).Assembly,
                typeof (IObjectQuerySerie).Assembly,
                typeof (IDb).Assembly,
                typeof (ScriptingEngine).Assembly,
                typeof (CSharpArgumentInfo).Assembly,
                typeof (DynamicObject).Assembly,
                typeof (ExpandoObject).Assembly,
                typeof (DbExtensions).Assembly,
            }.Distinct());

            _options = _options.WithImports(new[]
            {
                "System",
                "System.Collections.Generic",
                "System.Diagnostics",
                "System.Linq",
                "DbInterfaces.Interfaces",
                "Timeenator.Interfaces",
                "FileDb.Interfaces",
                "FileDb",
                "Timeenator.Impl",
                "Timeenator.Impl.Grouping",
                "Timeenator.Extensions.Grouping",
                "Timeenator.Extensions.Scientific",
                "Timeenator.Extensions.Converting",
                "Microsoft.CSharp",
                "System.Dynamic",
                "DbInterfaces.Interfaces",
                "CustomDbExtensions"
            });
            
        }

        private void CreateScript()
        {
            var scriptText = ScriptText;

            Script<IQueryResult> existingScript;

            if (_scripts.TryGetValue(scriptText, out existingScript))
            {
                _script = existingScript;
                return;
            }

            _script = CSharpScript.Create<IQueryResult>(scriptText, globalsType: typeof (Globals), options: _options);
            _script.Compile();
            _scripts[scriptText] = _script;
        }

        private string ScriptText => _expression;
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using DbInterfaces.Interfaces;
using FileDb.InterfaceImpl;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using QueryLanguage.Grouping;

namespace QueryLanguage.Scripting
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
                typeof (IDb).Assembly,
                typeof (Column).Assembly,
                typeof (FillExtensions).Assembly,
            }.Distinct());

            _options = _options.WithImports(new[]
            {
                "System",
                "System.Collections.Generic",
                "System.Diagnostics",
                "System.Linq",
                "DbInterfaces.Interfaces",
                "FileDb.InterfaceImpl",
                "QueryLanguage.Grouping",
                "QueryLanguage.Scientific",
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

        private string ScriptText => $"db.{_expression}";
    }
}

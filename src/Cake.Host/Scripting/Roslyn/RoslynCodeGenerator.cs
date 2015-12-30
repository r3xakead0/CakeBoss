﻿using System.Collections.Generic;
using Cake.Core.Scripting;
using Cake.Core.Scripting.CodeGen;

namespace Cake.Host.Scripting.Roslyn
{
    public sealed class RoslynCodeGenerator
    {
        public string Generate(Script script)
        {
            var usingDirectives = string.Join("\r\n", script.UsingAliasDirectives);
            var aliases = GetAliasCode(script);
            var code = string.Join("\r\n", script.Lines);
            return string.Join("\r\n", usingDirectives, aliases, code);
        }

        private static string GetAliasCode(Script context)
        {
            var result = new List<string>();
            foreach (var alias in context.Aliases)
            {
                result.Add(alias.Type == ScriptAliasType.Method
                    ? MethodAliasGenerator.Generate(alias.Method)
                    : PropertyAliasGenerator.Generate(alias.Method));
            }
            return string.Join("\r\n", result);
        }
    }
}
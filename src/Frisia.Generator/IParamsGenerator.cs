using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Frisia.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;

namespace Frisia.Generator
{
    public interface IParamsGenerator
    {
        (IEnumerable<Set> sets, SyntaxNode rewrittenNode, TimeSpan elapsed) ProvideParameters(MethodDeclarationSyntax method, ScriptState state);
        Task<(IEnumerable<Set> sets, SyntaxNode rewrittenNode, TimeSpan elapsed)> ProvideParametersAsync(string cSharpCode);
    }
}
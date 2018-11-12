using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;

namespace Frisia.Generator
{
    public interface IParamsGenerator
    {
        Task<(IEnumerable<Set> sets, SyntaxNode rewrittenNode, TimeSpan elapsed)> ProvideParametersAsync(MethodDeclarationSyntax method, ScriptState state);
        Task<(IEnumerable<Set> sets, SyntaxNode rewrittenNode, TimeSpan elapsed)> ProvideParametersAsync(string cSharpCode);
    }
}
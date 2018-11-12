using Frisia.Rewriter;
using Frisia.Solver;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Frisia.Generator
{
    public class ParamsGenerator : IParamsGenerator
    {
        private readonly ILogger logger;
        private readonly ISolver solver;
        private readonly uint loopIterations;
        private readonly bool visitUnsatisfiablePaths;
        private readonly bool logFoundBranches;

        public ParamsGenerator(ILogger logger, ISolver solver, uint loopIterations = 1, bool visitUnsatisfiablePaths = false, bool logFoundBranches = false)
        {
            this.logger = logger;
            this.solver = solver;
            this.loopIterations = loopIterations;
            this.visitUnsatisfiablePaths = visitUnsatisfiablePaths;
            this.logFoundBranches = logFoundBranches;
        }

        public async Task<(IEnumerable<Set> sets, SyntaxNode rewrittenNode, TimeSpan elapsed)> ProvideParametersAsync(string cSharpCode)
        {
            var totalSets = new List<Set>();
            logger.Info("Validating code...");
            var state = await CSharpScript.RunAsync(cSharpCode);
            
            var rewriterTimer = Stopwatch.StartNew();

            logger.Info("Analyzing code...");
            var rootNode = CSharpSyntaxTree.ParseText(cSharpCode).GetRoot();
            var methods = rootNode.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var m in methods)
            {
                var (sets, rewrittenNode, elapsed) = await ProvideParametersAsync(m, state);
                rootNode = rootNode.ReplaceNode(m, rewrittenNode);

                totalSets.AddRange(sets);
            }

            rewriterTimer.Stop();
            logger.Info($"Done in {rewriterTimer.Elapsed}");

            return (totalSets, rootNode, rewriterTimer.Elapsed);
        }

        public async Task<(IEnumerable<Set> sets, SyntaxNode rewrittenNode, TimeSpan elapsed)> ProvideParametersAsync(MethodDeclarationSyntax method, ScriptState state)
        {
            var sets = new List<Set>();
            var conditions = new List<ExpressionSyntax>();
            var sms = new SymbolicMemoryState(method.ParameterList.Parameters);
            var rewriter = new FrisiaSyntaxRewriter(
                conditions,
                method.ParameterList.Parameters,
                sms,
                solver,
                logger,
                loopIterations,
                visitUnsatisfiablePaths,
                logFoundBranches);

            var rewriterTimer = Stopwatch.StartNew();
            var rewrittenNode = (MethodDeclarationSyntax)rewriter.Visit(method);
            rewriterTimer.Stop();
            logger.Info($"{method.Identifier.Text} rewritten in {rewriterTimer.Elapsed}");

            IList<string[]> results = rewriter.GetResults();
            
            // Add default values
            var defaults = new string[method.ParameterList.Parameters.Count];
            for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
            {
                defaults[i] = TypeHelper.GetDefaultValue(method.ParameterList.Parameters[i].Type).ToString();
            }
            results.Add(defaults);

            foreach (var r in results)
            {
                var p = "";
                object returnValue = null;
                for (int i = 0; i < r.Length; i++)
                {
                    var parameter = method.ParameterList.Parameters.SingleOrDefault(x => x.Identifier.Text == r[i]);
                    if (parameter != null)
                    {
                        p += TypeHelper.GetDefaultValue(parameter.Type.ToString());
                    }
                    else
                    {
                        p += r[i];
                    }
                    if (i + 1 < r.Length)
                    {
                        p += ", ";
                    }
                }

                var className = (method.Parent as ClassDeclarationSyntax).Identifier.Text;
                try
                {
                    state = await state.ContinueWithAsync($"{className}.{method.Identifier.Text}({p})");
                    returnValue = state.ReturnValue;
                }
                catch (Exception ex)
                {
                    returnValue = ex.GetType().FullName;
                }

                var parameters = new Parameter[r.Length];

                for (int i = 0; i < r.Length; i++)
                {
                    parameters[i] = new Parameter
                    {
                        Value = r[i],
                        TypeName = TypeHelper.GetFullTypeName(method.ParameterList.Parameters[i].Type)
                    };
                }

                var s = new Set
                {
                    MethodName = method.Identifier.Text,
                    Parameters = parameters,
                    ResultTypeName = TypeHelper.GetFullTypeName(method.ReturnType),
                    ExpectedResult = returnValue
                };

                sets.Add(s);
            }

            return (sets, rewrittenNode, rewriterTimer.Elapsed);
        }
    }
}

using Frisia.Core;
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
        private readonly byte timeout;

        public ParamsGenerator(ILogger logger, ISolver solver, uint loopIterations = 1, bool visitUnsatisfiablePaths = false, bool logFoundBranches = false, byte timeout = 5)
        {
            this.logger = logger;
            this.solver = solver;
            this.loopIterations = loopIterations;
            this.visitUnsatisfiablePaths = visitUnsatisfiablePaths;
            this.logFoundBranches = logFoundBranches;
            this.timeout = timeout;
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
                var (sets, rewrittenNode, elapsed) = ProvideParameters(m, state);
                rootNode = rootNode.ReplaceNode(m, rewrittenNode);

                totalSets.AddRange(sets);
            }

            rewriterTimer.Stop();
            logger.Info($"Done in {rewriterTimer.Elapsed}");

            return (totalSets, rootNode, rewriterTimer.Elapsed);
        }

        public (IEnumerable<Set> sets, SyntaxNode rewrittenNode, TimeSpan elapsed) ProvideParameters(MethodDeclarationSyntax method, ScriptState state)
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
                var returnValue = Runner.Run(state, method, r, timeout);

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

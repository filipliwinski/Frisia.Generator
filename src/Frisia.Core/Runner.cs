using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Frisia.Core
{
    public class Runner
    {
        public static object Run(ScriptState state, MethodDeclarationSyntax method, string[] values, byte timeout)
        {
            var p = "";
            for (int i = 0; i < values.Length; i++)
            {
                var parameter = method.ParameterList.Parameters.SingleOrDefault(x => x.Identifier.Text == values[i]);
                if (parameter != null)
                {
                    p += TypeHelper.GetDefaultValue(parameter.Type.ToString());
                }
                else
                {
                    p += values[i];
                }
                if (i + 1 < values.Length)
                {
                    p += ", ";
                }
            }

            var className = (method.Parent as ClassDeclarationSyntax).Identifier.Text;
            try
            {
                var task = Task.Run(async () =>
                {
                    state = await state.ContinueWithAsync($"{className}.{method.Identifier.Text}({p})");
                    return state.ReturnValue;
                });

                bool isCompleted = task.Wait(timeout * 1000);

                if (isCompleted)
                {
                    return task.Result;
                }
                else
                {
                    throw new TimeoutException();
                }
            }
            catch (AggregateException ex)
            {
                return ex.InnerException.GetType().FullName;
            }
            catch (Exception ex)
            {
                return ex.GetType().FullName;
            }
        }
    }
}

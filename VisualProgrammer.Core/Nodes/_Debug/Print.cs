using System;
using System.Linq.Expressions;
using System.Reflection;

namespace VisualProgrammer.Core.Nodes.Debug
{
    public class Print : VisualStatement {

        [VisualNodeProperty] public ExpressionReference<string> PrintValue { get; set; }

        private static readonly MethodInfo write = typeof(Console).GetMethod("WriteLine", new[] { typeof(object) })!;

        public override Expression CreateExpression(VisualProgram context)
            => Expression.Call(null, write, PrintValue.ResolveRequiredExpression(context));
    }
}

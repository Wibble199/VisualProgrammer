using System;
using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Text {

    public class ToString<TSource> : VisualExpression<string> {

        [VisualNodeProperty]
        public ExpressionReference<TSource> Value { get; set; }

        public override Expression CreateExpression(VisualProgram context) => Expression.Call(
            Value.ResolveRequiredExpression(context),
            typeof(TSource).GetMethod("ToString", Array.Empty<Type>())
        );
    }
}

using System;
using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Text {

    public class ToString<TSource> : VisualExpression<string> {

        [VisualNodeExpressionProperty(nameof(TSource))]
        public NodeReference? Value { get; set; }

        public override Expression CreateExpression(VisualProgram context) => Expression.Call(
            Value.ResolveRequiredExpression(context),
            typeof(TSource).GetMethod("ToString", Array.Empty<Type>())
        );
    }
}

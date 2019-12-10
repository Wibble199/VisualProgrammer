using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Variables {

    public sealed class SetVariable<TVar> : VisualStatement {

		[VisualNodeProperty] public VariableReference<TVar> Variable { get; set; }
        [VisualNodeProperty] public ExpressionReference<TVar> Value { get; set; }

        public override Expression CreateExpression(VisualProgram context) =>
            VariableAccessorFactory.CreateSetterExpression(context, Variable, Value.ResolveRequiredExpression(context));
    }
}

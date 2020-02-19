using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Variables {

    public sealed class GetVariable<TVar> : VisualExpression<TVar> {

		[VisualNodeProperty] public VariableReference<TVar> Variable { get; set; }

		public override Expression CreateExpression(VisualProgram context) =>
			Variable.ResolveRequiredGetterExpression(context);
    }
}

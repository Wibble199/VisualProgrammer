using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Flow {

	public class Ternary<T> : VisualExpression<T> {

		[VisualNodeProperty(Order = -1)]
		public ExpressionReference<bool> Condition { get; set; }


		[VisualNodeProperty]
		public ExpressionReference<T> A { get; set; }


		[VisualNodeProperty]
		public ExpressionReference<T> B { get; set; }

		public override Expression CreateExpression(VisualProgram context) => Expression.Condition(
			Condition.ResolveRequiredExpression(context),
			A.ResolveRequiredExpression(context),
			B.ResolveRequiredExpression(context)
		);
	}
}

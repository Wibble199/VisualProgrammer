using System.Linq.Expressions;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core.Nodes.Flow {

	public class RangeLoop : VisualStatement {

		[VisualNodeProperty(Order = 0)] public VariableReference<double> Variable { get; set; }
		[VisualNodeProperty(Order = 1)] public ExpressionReference<double> Start { get; set; }
		[VisualNodeProperty(Order = 2)] public ExpressionReference<double> Change { get; set; }
		[VisualNodeProperty(Order = 3)] public ExpressionReference<double> End { get; set; }

		[VisualNodeProperty] public StatementReference Body { get; set; }

		public override Expression CreateExpression(VisualProgram context) {
			// If a start value is provided, set the target variable to that value. If not provided, do not
			var init = Start.HasValue
				? Variable.ResolveRequiredSetterExpression(context, Start.ResolveRequiredExpression(context))
				: Expression.Empty();

			// Store some re-used expressions to prevent re-resolving them
			var variableExpr = Variable.ResolveRequiredGetterExpression(context);
			var changeExpr = Change.ResolveExpression(context) ?? Expression.Constant(1d, typeof(double));
			var endExpr = End.ResolveRequiredExpression(context);

			var loop = LoopFactory.CreateLoop(
				// Loop body
				Expression.Block(
					// Run the given body
					Body.ResolveStatement(context),

					// Update the variable by 'Change' amount
					Variable.ResolveRequiredSetterExpression(
						context,
						Expression.Add(variableExpr, changeExpr)
					)
				),

				// Loop condition
				Expression.Condition(
					// If the 'Change' value is >= 0...
					Expression.GreaterThanOrEqual(
						changeExpr,
						Expression.Constant(0d, typeof(double))
					),
					// Then the condition is whether the variable is less than or equal to the end
					Expression.LessThanOrEqual(variableExpr, endExpr),
					// Otherwise (if 'Change' is < 0) the condition is whether variable is greater than or equal to the end
					Expression.GreaterThanOrEqual(variableExpr, endExpr)
				)
			);

			// Return a block that sets the initial value then executes the loop
			return Expression.Block(init, loop);
		}
	}
}

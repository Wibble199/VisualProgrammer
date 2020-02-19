using System.Linq.Expressions;
using static VisualProgrammer.Core.Nodes.Maths.Operation;

namespace VisualProgrammer.Core.Nodes.Maths {

	public class InlineOperation : VisualStatement {

		[VisualNodeProperty(Order = 1)] public VariableReference<double> Variable { get; set; }
		[VisualNodeProperty(Label = "Op", Order = 2)] public Op SelectedOperation { get; set; }
		[VisualNodeProperty(Order = 3)] public ExpressionReference<double> Value { get; set; }

		public override Expression CreateExpression(VisualProgram context) => Variable.ResolveRequiredSetterExpression(context, opMap[SelectedOperation](
				Variable.ResolveRequiredGetterExpression(context),
				Value.ResolveRequiredExpression(context)
			)
		);
	}
}

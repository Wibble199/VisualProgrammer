using System.Linq.Expressions;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core.Nodes.Flow {

    public class Loop : VisualStatement {

        [VisualNodeProperty] public ExpressionReference<bool> Condition { get; set; }
        [VisualNodeProperty] public StatementReference Body { get; set; }

		public override Expression CreateExpression(VisualProgram context) => LoopFactory.CreateLoop(
			Body.ResolveStatement(context),
			Condition.ResolveRequiredExpression(context)
		);
    }
}

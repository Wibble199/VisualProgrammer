using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Variables {

    public sealed class Literal<TValue> : VisualExpression<TValue> {

		[VisualNodeValueProperty]
		public TValue Value { get; set; }

        public override Expression CreateExpression(VisualProgram context) =>
            Expression.Constant(Value, typeof(TValue));
    }
}

using System.Linq.Expressions;
using System.Reflection;

namespace VisualProgrammer.Core.Nodes.Text {

    public class StringConcat : VisualExpression<string> {

        [VisualNodeProperty(Label = "A")] public ExpressionReference<string> First { get; set; }
        [VisualNodeProperty(Label = "B")] public ExpressionReference<string> Second { get; set; }

        private MethodInfo concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!;
        public override Expression CreateExpression(VisualProgram context) => Expression.Call(
            concat,
            First.ResolveExpression(context) ?? Expression.Constant(""),
            Second.ResolveExpression(context) ?? Expression.Constant("")
        );
    }
}

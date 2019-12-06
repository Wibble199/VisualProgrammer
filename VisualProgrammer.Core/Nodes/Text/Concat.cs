using System.Linq.Expressions;
using System.Reflection;

namespace VisualProgrammer.Core.Nodes.Text {

    public class StringConcat : VisualExpression<string> {

        [VisualNodeExpressionProperty(typeof(string), Label = "A")]
        public NodeReference? First { get; set; }

        [VisualNodeExpressionProperty(typeof(string), Label = "B")]
        public NodeReference? Second { get; set; }

        private MethodInfo concat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!;
        public override Expression CreateExpression(VisualProgram context) => Expression.Call(
            concat,
            First.ResolveExpression(context) ?? Expression.Constant(""),
            Second.ResolveExpression(context) ?? Expression.Constant("")
        );
    }
}

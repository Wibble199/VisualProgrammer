using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Maths {

    public class Operation : VisualExpression<double> {

        [VisualNodeProperty(Label = "A")] public ExpressionReference<double> LHS { get; set; }
        [VisualNodeProperty(Label = "B")] public ExpressionReference<double> RHS { get; set; }
        [VisualNodeProperty(Label = "Op")] public Op SelectedOperation { get; set; }

        public override Expression CreateExpression(VisualProgram context) => opMap[SelectedOperation](
            LHS.ResolveRequiredExpression(context),
            RHS.ResolveRequiredExpression(context)
        );

        public enum Op {
            Add,
            Substract,
            Multiply,
            Divide,
            Modulo,
            Pow
        }

        internal static readonly Dictionary<Op, Func<Expression, Expression, Expression>> opMap = new Dictionary<Op, Func<Expression, Expression, Expression>> {
            { Op.Add, Expression.Add },
            { Op.Substract, Expression.Subtract },
            { Op.Multiply, Expression.Multiply },
            { Op.Divide, Expression.Divide },
            { Op.Modulo, Expression.Modulo },
            { Op.Pow, Expression.Power }
        };
    }
}

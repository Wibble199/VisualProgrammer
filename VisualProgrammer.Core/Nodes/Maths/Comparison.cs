using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VisualProgrammer.Core.Nodes.Maths {

    public class Comparison : VisualExpression<bool> {

        [VisualNodeExpressionProperty(typeof(double), Label = "A")]
        public NodeReference? LHS { get; set; }

        [VisualNodeExpressionProperty(typeof(double), Label = "B")]
        public NodeReference? RHS { get; set; }

        [VisualNodeValueProperty(Label = "Op")]
        public Op SelectedOp { get; set; }

        public override Expression CreateExpression(VisualProgram context) => opMap[SelectedOp](
            LHS.ResolveRequiredExpression(context),
            RHS.ResolveRequiredExpression(context)
        );

        public enum Op {
            Eq,
            NEq,
            LT,
            LTE,
            GT,
            GTE
        }

        private static Dictionary<Op, Func<Expression, Expression, Expression>> opMap = new Dictionary<Op, Func<Expression, Expression, Expression>> {
            { Op.Eq, Expression.Equal },
            { Op.NEq, Expression.NotEqual },
            { Op.LT, Expression.LessThan },
            { Op.LTE, Expression.LessThanOrEqual },
            { Op.GT, Expression.GreaterThan },
            { Op.GTE, Expression.GreaterThanOrEqual }
        };
    }
}

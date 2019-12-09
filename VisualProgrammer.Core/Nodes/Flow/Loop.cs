﻿using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace VisualProgrammer.Core.Nodes.Flow {

    public class Loop : VisualStatement {

        [VisualNodeProperty] public ExpressionReference<bool> Condition { get; set; }
        [VisualNodeProperty] public StatementReference Body { get; set; }

       public override Expression CreateExpression(VisualProgram context) {
            var @break = Label("break");
            return Expression.Loop(
                Block(
                    IfThen(
                        Not(Condition.ResolveRequiredExpression(context)),
                        Break(@break)
                    ),
                    Body.ResolveStatement(context)
                ),
                @break
            );
        }
    }
}

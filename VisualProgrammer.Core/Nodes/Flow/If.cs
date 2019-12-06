using System.Linq.Expressions;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core.Flow {

    public sealed class If : VisualStatement {

        [VisualNodeExpressionProperty(typeof(bool))]
        public NodeReference? Condition { get; set; }

        [VisualNodeStatementProperty]
        public NodeReference? TrueBranch { get; set; }

        [VisualNodeStatementProperty]
        public NodeReference? FalseBranch { get; set; }

        public override Expression CreateExpression(VisualProgram context) {
            var branches = NodeUtils.FlattenExpressions(context, TrueBranch, FalseBranch);
            return Expression.IfThenElse(
                Condition.ResolveRequiredExpression(context),
                Expression.Block(branches.branch1Flattened),
                Expression.Block(branches.branch2Flattened)
            );
        }

        /* Override the default next statement so that: a) it is not visible in the editor (by not adding a VisualStatementLinkAttribute it
         * won't be); and b) so that we can override the default behaviour for when the visual compiler hits this node - since this If has
         * no idea where the if-end part is, we computed the first shared node from the two branches. Anything before this node is exclusive
         * to either the true/false branches, but anything including and after this shared node will be part of this "NextStatement". */
        public override NodeReference? NextStatement { get => null; set { } }
        public override NodeReference? GetCompilerNextStatement(VisualProgram context) => NodeUtils.FindNextSharedNode(context, TrueBranch, FalseBranch);
    }
}

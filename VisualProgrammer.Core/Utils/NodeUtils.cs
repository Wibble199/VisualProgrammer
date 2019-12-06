using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace VisualProgrammer.Core.Utils {

    internal static class NodeUtils {

        /// <summary>
        /// Finds the next node in common in the two branches. Can be used to help optimise the generation of branching flow statements
        /// such as if conditions, etc. Returns null if there is no common node.
        /// </summary>
        internal static NodeReference? FindNextSharedNode(VisualProgram context, NodeReference? branch1, NodeReference? branch2) {
            // Early return if either branch doesn't have any attached nodes - there cannot be a shared node if there are no nodes.
            if (!branch1.HasValue || !branch2.HasValue) return null;

            // Get a flat list of all Guids in the first branch
            var nodesIn1 = new List<Guid>();
            NodeReference? currentRef = branch1;
            while (currentRef != null) {
                nodesIn1.Add(currentRef.Value.nodeId);
                // Note that we use GetNextStatement here instead of NextStatement since, if it is a branch, we want the next shared statement of that branch
                currentRef = (currentRef.ResolveNode(context) as VisualStatement)?.GetCompilerNextStatement(context);
            }

            // Search the second branch until we find the first occurance of the same node in the first branch
            currentRef = branch2;
            while (currentRef != null) {
                if (nodesIn1.Contains(currentRef.Value.nodeId))
                    return currentRef.Value;
                currentRef = (currentRef.ResolveNode(context) as VisualStatement)?.GetCompilerNextStatement(context);
            }
            return null;
        }

        /// <summary>
        /// Starting at the given node, gets a list of <see cref="Expression"/>s that include and follow this node's references.
        /// </summary>
        internal static IEnumerable<Expression> FlattenExpressions(VisualProgram context, NodeReference? firstStatement) {
            var list = new List<Expression>();
            var currentNode = firstStatement.ResolveNode(context);
            while (currentNode is VisualStatement currentStatement) {
                list.Add(currentStatement.CreateExpression(context));
                currentNode = currentStatement.GetCompilerNextStatement(context).ResolveNode(context);
            }
            return list;
        }

        /// <summary>
        /// Starting at the given branch entry points, gets a list of <see cref="Expression"/>s that include and follow the branch, UNTIL a shared
        /// node is found. The shared node will not be included in the returned lists.
        /// </summary>
        internal static (IEnumerable<Expression> branch1Flattened, IEnumerable<Expression> branch2Flattened) FlattenExpressions(VisualProgram context, NodeReference? branch1, NodeReference? branch2) {
            var firstShared = FindNextSharedNode(context, branch1, branch2);

            // Early return if there is no shared node
            if (firstShared == null) return (FlattenExpressions(context, branch1), FlattenExpressions(context, branch2));

            List<Expression> flatten(NodeReference? start) {
                var list = new List<Expression>();
                var curRef = start;
                while (curRef.HasValue && curRef != firstShared && curRef.ResolveNode(context) is VisualStatement statement) {
                    list.Add(statement.CreateExpression(context));
                    curRef = statement.GetCompilerNextStatement(context);
                }
                return list;
            }
            return (flatten(branch1), flatten(branch2));
        }
    }
}

using System;
using System.Linq;
using System.Linq.Expressions;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core {

    /// <summary>
    /// Represents a link to another node in the VisualProgram.
    /// </summary>
    /// <remarks>This is a custom type so that the implementation can be changed in future if the need arises.</remarks>
    public readonly struct NodeReference {
        internal readonly Guid nodeId;

        /// <summary>
        /// Creates a new <see cref="NodeReference"/> by specifiying the ID directly.
        /// </summary>
        /// <param name="nodeId"></param>
        public NodeReference(Guid nodeId) {
            this.nodeId = nodeId;
        }

        /// <summary>
        /// Creates a new <see cref="NodeReference" /> by passing the string representation of the reference.
        /// </summary>
        /// <param name="nodeId"></param>
        public NodeReference(string nodeId) {
            this.nodeId = new Guid(nodeId);
        }

        /// <summary>
        /// Creates a new <see cref="NodeReference"/> by looking up the given node's key in the given program context.
        /// </summary>
        public NodeReference(VisualProgram context, VisualNode node) {
            nodeId = context.Nodes.First(n => Equals(n.Value, node)).Key;
        }

        public override string ToString() => nodeId.ToString();

        public override bool Equals(object? obj) => nodeId == (obj as NodeReference?)?.nodeId;
        public static bool operator ==(NodeReference a, NodeReference b) => a.nodeId == b.nodeId;
        public static bool operator !=(NodeReference a, NodeReference b) => a.nodeId != b.nodeId;
        public override int GetHashCode() => nodeId.GetHashCode();
    }


    /// <summary>
    /// Extension methods for the (nullabe form of) NodeReference.
    /// </summary>
    public static class NodeReferenceExtensions {

        /// <summary>Attempts to resolve this NodeReference. If the reference is null, returns null.</summary>
        public static VisualNode? ResolveNode(this NodeReference? nr, VisualProgram context) {
            VisualNode? node = null;
            if (nr.HasValue) context.TryResolveReference(nr.Value, out node);
            return node;
        }

		/// <summary>Attempts to resolve this NodeReference. If the reference is null, throws an error.</summary>
		/// <exception cref="Exception" />
		public static VisualNode ResolveRequiredNode(this NodeReference? nr, VisualProgram context)
			=> ResolveNode(nr, context) ?? throw new Exception(""); // TODO Add meaningful message


		/// <summary>Attempts to resolve this NodeReference's expression. If the reference is null, returns null.</summary>
		public static Expression? ResolveExpression(this NodeReference? nr, VisualProgram context) =>
            nr.ResolveNode(context)?.CreateExpression(context);

		/// <summary>Attempts to resolve this NodeReference's expression. If the reference is null, throws an error.</summary>
		/// <exception cref="Exception" />
		public static Expression ResolveRequiredExpression(this NodeReference? nr, VisualProgram context) =>
			nr.ResolveRequiredNode(context).CreateExpression(context);


		/// <summary>
		/// Attempts to generate a <see cref="BlockExpression"/> that contains the flattened sequence of statements referenced by this reference.
		/// If there is no first node, an empty block will be returned.
		/// </summary>
		/// <param name="nr">The nodereference to create the block from.</param>
		/// <param name="context">The execution context of the statements.</param>
		public static BlockExpression ResolveStatement(this NodeReference? nr, VisualProgram context) =>
            Expression.Block(expressions: NodeUtils.FlattenExpressions(context, nr));

		/// <summary>
		/// Attempts to generate a <see cref="BlockExpression"/> that contains the flattened sequence of statements referenced by this reference.
		/// If there is no first node, an exception will be thrown.
		/// </summary>
		/// <param name="nr">The nodereference to create the block from.</param>
		/// <param name="context">The execution context of the statements.</param>
		/// <exception cref="Exception" />
		public static BlockExpression ResolveRequiredStatement(this NodeReference? nr, VisualProgram context) {
			var body = NodeUtils.FlattenExpressions(context, nr);
			if (body.Count() == 0) throw new Exception(""); // TODO Add meaningful message
			return Expression.Block(expressions: body);
		}
	}
}

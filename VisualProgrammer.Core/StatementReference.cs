using System;
using System.Linq;
using System.Linq.Expressions;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core {

    /// <summary>
    /// Represents a link to another statement node in the VisualProgram.
    /// </summary>
    public readonly struct StatementReference : INodeReference {
        internal readonly Guid? nodeId;
	
		/// <summary>
		 /// Creates a new <see cref="StatementReference"/> by specifiying the ID directly.
		 /// </summary>
		public StatementReference(Guid nodeId) {
            this.nodeId = nodeId;
        }

        /// <summary>
        /// Creates a new <see cref="StatementReference" /> by passing the string representation of the reference.
        /// </summary>
        public StatementReference(string nodeId) {
            this.nodeId = new Guid(nodeId);
        }

        /// <summary>
        /// Creates a new <see cref="StatementReference"/> by looking up the given node's key in the given program context.
        /// </summary>
        public StatementReference(VisualNode node) {
            nodeId = node.Id;
        }

		public VisualNode? ResolveNode(VisualProgram context) => context.Nodes[nodeId];
		public VisualNode ResolveRequiredNode(VisualProgram context) => ResolveNode(context) ?? throw new Exception();

		public Expression? ResolveExpression(VisualProgram context) => ResolveNode(context)?.CreateExpression(context);
		public Expression ResolveRequiredExpression(VisualProgram context) => ResolveExpression(context) ?? throw new Exception();

		public bool HasValue => nodeId.HasValue;
		public Guid? Id => nodeId;

		/// <summary>Resolves this reference into a single <see cref="BlockExpression"/> containing all the sequential statements.</summary>
		public BlockExpression ResolveStatement(VisualProgram context) => Expression.Block(expressions: NodeUtils.FlattenExpressions(context, this));

		/// <summary>Resolves this reference into a single <see cref="BlockExpression"/> containing all the sequential statements.
		/// If there are no statements that would end up in the block, throws an exception.</summary>
		/// <exception cref="Exception"></exception>
		public BlockExpression ResolveRequiredStatement(VisualProgram context) {
			var body = NodeUtils.FlattenExpressions(context, this);
			if (body.Count() == 0) throw new Exception(); // TODO Add meaningful message
			return Expression.Block(expressions: body);
		}

		public override string ToString() => nodeId.ToString();

        public override bool Equals(object? obj) => nodeId == (obj as StatementReference?)?.nodeId;
        public static bool operator ==(StatementReference a, StatementReference b) => a.nodeId == b.nodeId;
        public static bool operator !=(StatementReference a, StatementReference b) => a.nodeId != b.nodeId;
        public override int GetHashCode() => nodeId.GetHashCode();
	}
}

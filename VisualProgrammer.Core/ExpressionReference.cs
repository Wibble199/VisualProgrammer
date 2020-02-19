using System;
using System.Linq.Expressions;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Represents an expression resulting in a value or links to another expression node in the <see cref="VisualProgram"/>.
	/// </summary>
	/// <typeparam name="TValue">The type of value that is expected by the expression.</typeparam>
	public struct ExpressionReference<TValue> : INodeReference {
		internal bool isValue;
		internal Guid? nodeId;
		internal TValue value;

#pragma warning disable CS8653
		/// <summary>
		/// Initializes a new instance of <see cref="ExpressionReference{TValue}"/> that points to a node with the given ID.
		/// </summary>
		/// <param name="nodeId">The GUID of the target node.</param>
		public ExpressionReference(Guid nodeId) {
			isValue = false;
			this.nodeId = nodeId;
			value = default;
		}

		/// <summary>
		/// Initializes a new instance of <see cref="ExpressionReference{TValue}"/> that points to a node with the given ID.<para/>
		/// Be aware that this may be called instead of <see cref="ExpressionReference{TValue}.ExpressionReference(TValue)" /> when <typeparamref name="TValue"/> is <see cref="string"/>.
		/// To prevent this, use: <code>new ExpressionReference&lt;string&gt;(value: "Some Value")</code>
		/// </summary>
		/// <param name="nodeId">The GUID of the target node.</param>
		public ExpressionReference(string nodeId) : this(new Guid(nodeId)) { }

		/// <summary>
		/// Initializes a new instance of <see cref="ExpressionReference{TValue}"/> that points to the given node within the context of the given program.
		/// </summary>
		public ExpressionReference(VisualNode node) {
			isValue = false;
			nodeId = node.Id;
			value = default;
		}

		/// <summary>
		/// Initializes a new instance of <see cref="ExpressionReference{TValue}"/> that will resolve to be a constant expression of the given value.<para/>
		/// Note that when 
		/// </summary>
		/// <param name="value">The value.</param>
		public ExpressionReference(TValue value) {
			isValue = true;
			nodeId = null;
			this.value = value;
		}
#pragma warning restore CS8653

		public VisualNode? ResolveNode(VisualProgram context) => isValue || !nodeId.HasValue ? null : context.Nodes[nodeId.Value];
		public VisualNode ResolveRequiredNode(VisualProgram context) => ResolveNode(context) ?? throw new Exception();

		public Expression? ResolveExpression(VisualProgram context) => isValue
			? Expression.Constant(value, typeof(TValue))
			: nodeId.HasValue ? context.Nodes[nodeId.Value]?.CreateExpression(context) : null;
		public Expression ResolveRequiredExpression(VisualProgram context) => ResolveExpression(context) ?? throw new Exception();

		public bool HasValue => isValue || nodeId.HasValue;
		public Guid? Id => nodeId;

		public override string ToString() => isValue ? value?.ToString() ?? "null value" : nodeId?.ToString() ?? default(Guid).ToString();

		public override bool Equals(object obj)
			=> obj is ExpressionReference<TValue> other && isValue == other.isValue && nodeId == other.nodeId && Equals(value, other.value);
		public static bool operator ==(ExpressionReference<TValue> a, ExpressionReference<TValue> b) => a.Equals(b);
		public static bool operator !=(ExpressionReference<TValue> a, ExpressionReference<TValue> b) => !a.Equals(b);
		public override int GetHashCode() => HashCode.Combine(isValue, nodeId, value);
	}

	/// <summary>Contains helper methods for <see cref="ExpressionReference{TValue}"/>.</summary>
	public static class ExpressionReference {
		/// <summary>Creates a new <see cref="ExpressionReference{TValue}"/> of the given type that points to the given node.</summary>
		/// <param name="refType">The inner data type of the expression reference.</param>
		/// <param name="node">The node that the reference will point to.</param>
		public static INodeReference Create(Type refType, VisualNode node) =>
			(INodeReference)Activator.CreateInstance(typeof(ExpressionReference<>).MakeGenericType(refType), node);
	}
}

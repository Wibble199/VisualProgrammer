using System;
using System.Linq.Expressions;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Represents a contract for <see cref="StatementReference"/>s and <see cref="ExpressionReference{T}"/>s to implement.
	/// </summary>
	public interface INodeReference {

		/// <summary>Resolves this reference into a concrete <see cref="VisualNode"/> from the given program context.
		/// If the node could not be found in the program, null is returned.</summary>
		VisualNode? ResolveNode(VisualProgram context);

		/// <summary>Resolves this reference into a concrete <see cref="VisualNode"/> from the given program context.
		/// If the node could not be found in the program, an exception will be thrown.</summary>
		/// <exception cref="Exception" />
		VisualNode ResolveRequiredNode(VisualProgram context);

		/// <summary>Resolves this reference into it's relevant <see cref="VisualNode"/>'s Linq expression. May recursivley resolve any child expressions.
		/// If the node could not be found in the program, null is returned.</summary>
		Expression? ResolveExpression(VisualProgram context);

		/// <summary>Resolves this reference into it's relevant <see cref="VisualNode"/>'s Linq expression. May recursivley resolve any child expressions.
		/// If the node could not be found in the program, an exception will be thrown.</summary>
		/// <exception cref="Exception" />
		Expression ResolveRequiredExpression(VisualProgram context);

		/// <summary>Whether or not this reference should successfully resolve. Note that this may not always mean that it will resolve successfully.</summary>
		bool HasValue { get; }

		/// <summary>The backing ID of this reference. May be used for determining the IDs of nodes in the UI.</summary>
		Guid? Id { get; }
	}
}

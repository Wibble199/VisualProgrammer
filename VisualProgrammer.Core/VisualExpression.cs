using System;

namespace VisualProgrammer.Core {

	public abstract class VisualExpression<TResult> : VisualNode, IVisualExpression {
		public Type ReturnType => typeof(TResult);
	}

	public interface IVisualExpression : IVisualNode {
		Type ReturnType { get; }
	}
}

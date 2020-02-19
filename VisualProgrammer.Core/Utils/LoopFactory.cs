using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace VisualProgrammer.Core.Utils {

	/// <summary>
	/// Provides methods to help with building expression loops.
	/// </summary>
	internal static class LoopFactory {

		/// <summary>
		/// Creates a loop that will execute the given body while the given condition is met.
		/// </summary>
		/// <param name="body">The expression that will be repeatedly be executed while condition is true.</param>
		/// <param name="condition">The condition that determines whether the body should keep executing.</param>
		public static Expression CreateLoop(Expression body, Expression condition) {
			var @break = Label("break");
			return Loop(
				Block(
					IfThen(
						Not(condition),
						Break(@break)
					),
					body
				),
				@break
			);
		}
	}
}

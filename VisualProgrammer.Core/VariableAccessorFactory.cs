using System;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Providers methods that create expressions relating to reading and writing variables.
	/// </summary>
    internal static class VariableAccessorFactory {

		/// <summary>
		/// Creates a property accessing expression that points to the variable with the given reference.
		/// </summary>
		/// <param name="context">The program context whose variable dictionary will be used.</param>
		/// <param name="variable">A reference to the desired variable.</param>
		private static Expression GetMemberExpressionFor(VisualProgram context, IVariableReference variable) =>
            Property(
				Property(
					PropertyOrField(
						context.Variables.compiledInstanceParameter,
						"variables"
					),
					"Item",
					Constant(variable.Name, typeof(string))
				),
				nameof(Variable.Value)
			);

		/// <summary>
		/// Creates an expression that gets the value of a visual program variable.
		/// </summary>
		/// <param name="context">The program context whose variable dictionary will be used.</param>
		/// <param name="variable">A reference to the variable.</param>
		/// <param name="type">The type that the variable should be cast to.</param>
		/// <exception cref=""></exception>
		internal static Expression CreateGetterExpression(VisualProgram context, IVariableReference variable, Type type) {
			variable.Validate(context);
			return Convert(GetMemberExpressionFor(context, variable), type);
		}

		/// <summary>
		/// Creates an expression that sets the variable to the given expression value.
		/// </summary>
		/// <param name="context">The program context whose variable dictionary will be used.</param>
		/// <param name="variable">A reference to the variable.</param>
		/// <param name="value">The expression that represents the new value of the variable.</param>
		internal static Expression CreateSetterExpression(VisualProgram context, IVariableReference variable, Expression value) {
			variable.Validate(context);
			return Assign(GetMemberExpressionFor(context, variable), Convert(value, typeof(object)));
		}
	}
}

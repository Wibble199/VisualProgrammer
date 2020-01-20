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
						context.compiledInstanceParameter,
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

        /// <summary>
        /// Creates an expression that gets the target variable, performs an action on it, sets it back to the variable store and returns the new value.
        /// This can be used to implement nodes such as increment, addassign, etc.
        /// </summary>
        /// <param name="context">The program context whose variable dictionary will be used.</param>
        /// <param name="variable">A reference to a variable.</param>
        /// <param name="expressionFactory">Factory function to generate the expression that will be performed between reading and writing the variable.
        /// It is passed the variable expression that can be read/written to. This factory MUST NOT return an assignment expression. For example, return
        /// `Expression.Add(...)` instead of `Expression.AddAssign(...)`.</param>
        internal static Expression CreateAssignmentExpression(VisualProgram context, IVariableReference variable, Func<Expression, Expression> expressionFactory, Type type) {
			variable.Validate(context);
            // If the factory returned an "Add" expression, the pseduocode generated would be `dict["myvar"] = dict["myvar"] + y`, which is a valid statement and a valid expression.
            var @var = GetMemberExpressionFor(context, variable);
            return Assign(
                var, // Assign to the relevant index in the dictionary
                Convert(
                    expressionFactory( // Create the assignment expression from the expression returned by the factory
                        Convert(var, type) // Pass an expression to the factory that gets the relevant var from the dictionary and then converts it to the specified type
                    ),
                    typeof(object) // The dictionary is <string, object> so we must explicitly convert to an object
                )
            );
        }
	}
}

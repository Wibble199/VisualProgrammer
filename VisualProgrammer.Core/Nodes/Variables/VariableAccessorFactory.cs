using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace VisualProgrammer.Core.Nodes.Variables {

    internal static class VariableAccessorFactory {

        private static IndexExpression GetMemberExpressionFor(VisualProgram context, string variableName) =>
            Property(
                Constant(context.VariableValues, typeof(Dictionary<string, object>)),
                "Item",
                Constant(variableName, typeof(string))
            );

        /// <summary>
        /// Creates an expression that gets the value of a visual program variable.
        /// </summary>
        /// <param name="context">The program context whose variable dictionary will be used.</param>
        /// <param name="variableName">The name of the variable in the dictionary.</param>
        /// <param name="type">The type that the variable should be cast to.</param>
        internal static Expression CreateGetterExpression(VisualProgram context, string variableName, Type type) =>
            Convert(GetMemberExpressionFor(context, variableName), type);

        /// <summary>
        /// Creates an expression that sets the variable to the given expression value.
        /// </summary>
        /// <param name="context">The program context whose variable dictionary will be used.</param>
        /// <param name="variableName">The name of the variable in the dictionary.</param>
        /// <param name="value">The expression that represents the new value of the variable.</param>
        internal static Expression CreateSetterExpression(VisualProgram context, string variableName, Expression value) =>
            Assign(GetMemberExpressionFor(context, variableName), Convert(value, typeof(object)));

        /// <summary>
        /// Creates an expression that gets the target variable, performs an action on it, sets it back to the variable store and returns the new value.
        /// This can be used to implement nodes such as increment, addassign, etc.
        /// </summary>
        /// <param name="context">The program context whose variable dictionary will be used.</param>
        /// <param name="variableName">The name of the variable in the dictionary.</param>
        /// <param name="expressionFactory">Factory function to generate the expression that will be performed between reading and writing the variable.
        /// It is passed the variable expression that can be read/written to. This factory MUST NOT return an assignment expression. For example, return
        /// `Expression.Add(...)` instead of `Expression.AddAssign(...)`.</param>
        internal static Expression CreateAssignmentExpression(VisualProgram context, string variableName, Func<Expression, Expression> expressionFactory, Type type) {
            // If the factory returned an "Add" expression, the pseduocode generated would be `dict["myvar"] = dict["myvar"] + y`, which is a valid statement and a valid expression.
            var @var = GetMemberExpressionFor(context, variableName);
            return Assign(
                var, // Assign to the relevant index in the dictionary
                Convert(
                    expressionFactory( // Create the assignment expression from the expression returned by the factory
                        Convert(var, type) // Pass an expression to the factory that gets the relevant var from the dictionary and then converts it to the specified type
                    ),
                    typeof(object) // The dictionary is <string, object> so we must convert to an object
                )
            );
        }
    }
}

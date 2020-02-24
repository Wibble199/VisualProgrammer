using System;
using System.Linq.Expressions;
using System.Reflection;
using VisualProgrammer.Core.Compilation;
using static System.Linq.Expressions.Expression;

namespace VisualProgrammer.Core.Utils {

	/// <summary>
	/// Providers methods that create expressions relating to reading and writing variables.
	/// </summary>
	internal static class VariableAccessorFactory {

		private static readonly MethodInfo getVariableMethod = typeof(ICompiledInstanceBase).GetMethod(nameof(ICompiledInstanceBase.GetVariable));
		private static readonly MethodInfo setVariableMethod = typeof(ICompiledInstanceBase).GetMethod(nameof(ICompiledInstanceBase.SetVariable));

		/// <summary>
		/// Creates an expression that gets the value of a visual program variable.
		/// </summary>
		/// <param name="context">The program context whose variable dictionary will be used.</param>
		/// <param name="variable">A reference to the variable.</param>
		/// <param name="type">The type that the variable should be cast to.</param>
		/// <exception cref=""></exception>
		internal static Expression CreateGetterExpression(VisualProgram context, IVariableReference variable, Type type) {
			variable.Validate(context);
			return Convert(
				Call(
					context.Variables.compiledInstanceParameter,
					getVariableMethod,
					Constant(variable.Name, typeof(string))
				),
				type
			);
		}

		/// <summary>
		/// Creates an expression that sets the variable to the given expression value.
		/// </summary>
		/// <param name="context">The program context whose variable dictionary will be used.</param>
		/// <param name="variable">A reference to the variable.</param>
		/// <param name="value">The expression that represents the new value of the variable.</param>
		internal static Expression CreateSetterExpression(VisualProgram context, IVariableReference variable, Expression value) {
			variable.Validate(context);
			return Call(
				context.Variables.compiledInstanceParameter,
				setVariableMethod,
				Constant(variable.Name, typeof(string)),
				Convert(value, typeof(object))
			);
		}
	}
}

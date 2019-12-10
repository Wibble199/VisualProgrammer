using System;
using System.Linq.Expressions;
using System.Reflection;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Contains data about a property that has been annotated with <see cref="VisualNodePropertyAttribute"/> and defined on a class extending a <see cref="VisualNode"/>.
	/// </summary>
	public class VisualNodePropertyDefinition {

		/// <summary>The name in code-behind of the property.</summary>
		public string Name { get; }

		/// <summary>Gets the display name of the property. Either the label or the code-behind name.</summary>
		public string DisplayName => Meta.Label ?? Name;

		/// <summary>The type of property this is recognised as by the VisualProgrammer. Either a value or a reference to a statement, expression or variable.</summary>
		public VisualNodePropertyType PropertyType { get; }

		/// <summary>The type of data stored inside the property. The exact type this refers to depends on the <see cref="PropertyType"/>:
		/// <list type="bullet">
		///		<item><term>Value</term><description>The type of value that is defined. E.G. if the property <c>public string X { get; set; }</c> was defined on the node, the <see cref="PropertyDataType"/> would be <c>string</c>.</description></item>
		///		<item><term>Expression</term><description>The type of value that is expected by the expression. E.G. if the property <c>public ExpressionReference&lt;double&gt;</c> was defined, <see cref="PropertyDataType"/> would be <c>double</c>.</description></item>
		///		<item><term>Statement</term><description>Always <c>null</c>.</description></item>
		///		<item><term>Variable</term><description>The type of value that is allowed by the variable reference. E.G. if the property <c>public VariableReference&lt;bool&gt;</c> was defined, <see cref="PropertyDataType"/> would be <c>bool</c>.</description></item>
		/// </list></summary>
		public Type? PropertyDataType { get; }

		/// <summary>The actual type of the defined property on the Node class. This may be a <see cref="ExpressionReference{TValue}"/>, <see cref="StatementReference"/> or <see cref="VariableReference{TVar}"/>.</summary>
		public Type RawType { get; }

		/// <summary>Metadata defined on the property attribute of this property definition.</summary>
		public VisualNodePropertyAttribute Meta { get; }

		/// <summary>A function that gets the value of this property on the given instance (arg0).</summary>
		public Func<object, object> Getter { get; }

		/// <summary>An action that sets the value of this property on the given instance (arg0) to the given value (arg1).</summary>
		public Action<object, object> Setter { get; }

        public VisualNodePropertyDefinition(PropertyInfo prop, VisualNodePropertyAttribute meta) {
            Name = prop.Name;
            Meta = meta;
			RawType = prop.PropertyType;

			// If determine PropertyType and PropertyDataType values from the type of the annotated property
			if (prop.PropertyType == typeof(StatementReference)) {
				PropertyType = VisualNodePropertyType.Statement;
				PropertyDataType = null;
			} else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ExpressionReference<>)) {
				PropertyType = VisualNodePropertyType.Expression;
				PropertyDataType = prop.PropertyType.GetGenericArguments()[0];
			} else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(VariableReference<>)) {
				PropertyType = VisualNodePropertyType.Variable;
				PropertyDataType = prop.PropertyType.GetGenericArguments()[0];
			} else {
				PropertyType = VisualNodePropertyType.Value;
				PropertyDataType = prop.PropertyType;
			}

            // Create a getter for this property
            var arg0 = Expression.Parameter(typeof(object));
            Getter = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.Property(
                        Expression.TypeAs(arg0, prop.DeclaringType),
                        prop.Name
                    ),
                    typeof(object)
                ),
                arg0
            ).Compile();

            // Create a setter for this property
            var arg1 = Expression.Parameter(typeof(object));
            Setter = Expression.Lambda<Action<object, object>>(
                Expression.Assign(
                    Expression.Property(
                        Expression.TypeAs(arg0, prop.DeclaringType),
                        prop.Name
                    ),
                    Expression.Convert(arg1, prop.PropertyType)
                ),
                arg0, arg1
            ).Compile();
        }
    }
}

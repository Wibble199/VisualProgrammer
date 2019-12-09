using System;
using System.Linq.Expressions;
using System.Reflection;

namespace VisualProgrammer.Core {

    public class VisualNodePropertyDefinition {

        public string Name { get; }
        public VisualNodePropertyType PropertyType { get; }
        public Type? PropertyDataType { get; }
        public VisualNodePropertyAttribute Meta { get; }
        public Func<object, object> Getter { get; }
        public Action<object, object> Setter { get; }

        public VisualNodePropertyDefinition(PropertyInfo prop, VisualNodePropertyAttribute meta) {
            Name = prop.Name;
            Meta = meta;

			// If determine PropertyType and PropertyDataType values from the type of the annotated property
			if (prop.PropertyType == typeof(StatementReference)) {
				PropertyType = VisualNodePropertyType.Statement;
				PropertyDataType = null;
			} else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(ExpressionReference<>)) {
				PropertyType = VisualNodePropertyType.Expression;
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

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace VisualProgrammer.Core {

    public class VisualNodePropertyDefinition {

        public string Name { get; }
        public VisualNodePropertyType PropertyType => Meta.PropertyType;
        public Type? PropertyDataType { get; }
        public VisualNodePropertyAttribute Meta { get; }
        public Func<object, object> Getter { get; }
        public Action<object, object> Setter { get; }

        public VisualNodePropertyDefinition(PropertyInfo prop, VisualNodePropertyAttribute meta) {
            Name = prop.Name;
            Meta = meta;

            PropertyDataType = meta.PropertyType switch {
                VisualNodePropertyType.Value => prop.PropertyType,
                VisualNodePropertyType.Expression => meta.GetExpressionType(prop.DeclaringType!),
                _ => null,
            };

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

using System;
using System.Linq;

namespace VisualProgrammer.Core {
       
    /// <summary>
    /// Attribute used to decorate properties on a <see cref="VisualNode" /> to allow them to be visible.
    /// </summary>
    public abstract class VisualNodePropertyAttribute : Attribute {

        protected Type expressionType;
        protected string expressionGenericTypeName;

		/// <summary></summary>
		/// <param name="propertyType">The type of property that this is.</param>
		/// <param name="expressionType">The type of data that should be accepted by this link. Should be null for non-expression properties.</param>
#pragma warning disable CS8618
		public VisualNodePropertyAttribute(VisualNodePropertyType propertyType) {
			PropertyType = propertyType;
        }
#pragma warning restore CS8618

		/// <summary>
		/// The type of property this represents. Either a raw value (that the user can manually enter), a reference to an expression or a reference to a statement.
		/// </summary>
		public VisualNodePropertyType PropertyType { get; protected set; }

        /// <summary>
        /// For expressions only, the type of data that should be accepted by the expression link.
        /// </summary>
        public Type GetExpressionType(Type declaringType) => !string.IsNullOrEmpty(expressionGenericTypeName) && declaringType.IsGenericType
            ?  declaringType.GetGenericArguments()[ // Get a list of the closed generic's type arguments, and the get the item at the index of...
                declaringType.GetGenericTypeDefinition()
                    .GetGenericArguments() // Get a list of the open (untyped) generic's type arguments
                    .Select((type, i) => (type, i))
                    .First(arg => arg.type.Name == expressionGenericTypeName).i // where the generic argument name is the generic type name provided.
            ] : expressionType; // If this does not reference a type argument, return the type variable

        /// <summary>
        /// A label for this property. If not set, the name of the property as defined in the code will be used.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// The order in which the property will appear in the auto-generated UI for the node.<para/>
        /// </summary>
        public int Order { get; set; }
    }


    /// <summary>
    /// Attribute used to decorate properties on a <see cref="VisualNode"/> which can be set directly by the user (e.g. strings, numbers, etc.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VisualNodeValuePropertyAttribute : VisualNodePropertyAttribute {
        public VisualNodeValuePropertyAttribute() : base(VisualNodePropertyType.Value) { }
    }

    /// <summary>
    /// Attribute used to decorate properties on a <see cref="VisualNode"/> which can be expressions that return a particular data type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VisualNodeExpressionPropertyAttribute : VisualNodePropertyAttribute {

        /// <summary>
        /// Indicates that this property should accept the given static type.
        /// </summary>
        public VisualNodeExpressionPropertyAttribute(Type exprType) : base(VisualNodePropertyType.Expression) {
            expressionType = exprType ?? throw new ArgumentNullException($"When using {nameof(VisualNodeExpressionPropertyAttribute)}, {nameof(exprType)} must be a non-null value.");
        }

        /// <summary>
        /// Indicates that this property should accept a type that is defined by the generic argument with this name that is defined on the
        /// declaring class.<para/>
        /// It is recommended to use `nameof` when referencing the type argument name, instead of using a 'magic string'.
        /// </summary>
        /// <remarks>This is required because attributes cannot use `typeof(T)` on generic arguments, so we instead resolve the type at runtime
        /// and bypassing the name of the generic argument instead.</remarks>
        public VisualNodeExpressionPropertyAttribute(string typeArgumentName) : base(VisualNodePropertyType.Expression) {
            expressionGenericTypeName = typeArgumentName ?? throw new ArgumentNullException($"When using {nameof(VisualNodeExpressionPropertyAttribute)}, {nameof(typeArgumentName)} must be a non-null value.");
        }
    }

    /// <summary>
    /// Attribute used to decorate properties on a <see cref="VisualNode"/> which represent links to other statements (e.g. the next statement, branching statements, etc.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class VisualNodeStatementPropertyAttribute : VisualNodePropertyAttribute {
        public VisualNodeStatementPropertyAttribute() : base(VisualNodePropertyType.Statement) { }
    }
    /* Note that the "Inherited" property is set to false so that when visual statements/expressions override a member, it doesn't show up unless the attribute is reapplied.
     * For example, the If conditional needs to override the next statement member so that it doesn't show up (as it doesn't make sense in that context). */


    public enum VisualNodePropertyType {
        /// <summary>This visual node property accepts a raw value (such as a string, int, etc).</summary>
        Value,
        /// <summary>This visual node property accepts a reference to an expression of a particular type.</summary>
        Expression,
        /// <summary>This visual node property accepts a reference to a statement.</summary>
        Statement
    }
}

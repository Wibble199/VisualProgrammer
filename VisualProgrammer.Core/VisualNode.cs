using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PropDef = VisualProgrammer.Core.VisualNodePropertyDefinition;

namespace VisualProgrammer.Core {

	/// <summary>
	/// The base class for all statements and expressions.
	/// </summary>
	public abstract class VisualNode : IVisualNode {

        /// <summary>A cache of all properties on this visual node type.</summary>
        private Dictionary<string, PropDef>? properties;

        /// <summary>The position of this node on the program canvas.</summary>
        public Point Position { get; set; }

        /// <summary>Gets the Linq Expression for this node.</summary>
        /// <param name="context">The program context. Required for variables, resolving links, etc.</param>
        public abstract Expression CreateExpression(VisualProgram context);



        /// <summary>Gets the properties on this VisualNode. These results are cached and subsequent calls will be quicker.</summary>
        private Dictionary<string, PropDef> Properties => properties ?? (properties =
            (from prop in GetType().GetProperties()
            let meta = prop.GetCustomAttribute<VisualNodePropertyAttribute>()
            where meta != null
            select (prop.Name, new PropDef(prop, meta))).ToDictionary(pair => pair.Name, pair => pair.Item2)
        );

        /// <summary>Gets all the properties of a specific <see cref="VisualNodePropertyType"/>, for example getting all statements.</summary>
        /// <param name="type">The type of property to filter by.</param>
        public IEnumerable<PropDef> GetPropertiesOfType(VisualNodePropertyType type) => from prop in Properties let def = prop.Value where def.PropertyType == type select def;



        /// <summary>Gets the value of the property with the given name.</summary>
        /// <exception cref="ArgumentException">If the property with the given name is not found.</exception>
        public object GetPropertyValue(string propertyName) {
            if (Properties.TryGetValue(propertyName, out var def))
                return def.Getter(this);
            else
                throw new ArgumentException($"Property with name '{propertyName}' was not found on this type {GetType().Name}.", nameof(propertyName));
        }

        /// <summary>Sets the value of the property with the given name to the given value.</summary>
        /// <remarks>Note that no type checking is performed, only casting.</remarks>
        /// <exception cref="ArgumentException">If the property with the given name is not found.</exception>
        public void SetPropertyValue(string propertyName, object value) {
            if (Properties.TryGetValue(propertyName, out var def))
                def.Setter(this, value);
            else
                throw new ArgumentException($"Property with name '{propertyName}' was not found on this type {GetType().Name}.", nameof(propertyName));
        }



        /// <summary>
        /// Attempts to link the given target <see cref="VisualNode"/> to the target property of this node.<para/>
        /// Will perform validation checks to ensure that the nodes can be linked, such as checking for expression/statement and expression type.
        /// </summary>
        /// <exception cref="ArgumentException">If the target property could not be found on this node.</exception>
        /// <exception cref="VisualNodeLinkException">When attempting to create a link that would be invalid, e.g. causing a circular reference, mismatching expression/statement types.</exception>
        public void Link(VisualProgram context, string targetProperty, VisualNode node) {
            // First, check the specified property exists on this node
            if (!Properties.TryGetValue(targetProperty, out var prop))
                throw new ArgumentException($"Property could not be found on type '{GetType().Name}'.", nameof(targetProperty));

            // Next, check that the property on this node accepts the type of incoming node
            // Also check that this link would not make a circular reference (e.g. a -> b -> c -> a or a -> a)
            switch (prop.PropertyType) {
                case VisualNodePropertyType.Expression:
                    // When prop is expression, check incoming is expression
                    if (!(node is IVisualExpression expr))
                        throw new VisualNodeLinkException($"Cannot link a non-expression VisualNode to the expression type property '{targetProperty}'.");
                    // Check that the expected expression return type matches the incoming expression return type
                    if (expr.ReturnType != prop.PropertyDataType)
                        throw new VisualNodeLinkException($"Cannot link an expression that returns '{expr.ReturnType.Name}' to a property that expects '{prop.PropertyDataType?.Name}'.");

                    // Check that this node is not part of the child tree of expressions of the incoming node.
                    // Since expressions cannot have child statements, we don't need to worry about those.
                    ValidateCircular<IVisualExpression>(context, expr, VisualNodePropertyType.Expression);

#warning TODO: Remove other links if required

					// If we got here, validation has passed so make the link
					SetPropertyValue(targetProperty, Activator.CreateInstance(typeof(ExpressionReference<>).MakeGenericType(prop.PropertyDataType), context, node));
					break;

                case VisualNodePropertyType.Statement:
                    // When prop is statement, check incoming is statement
                    if (!(node is VisualStatement stmt))
                        throw new VisualNodeLinkException($"Cannot link a non-statement VisualNode to the statement type property '{targetProperty}'.");

                    // Check this node is not part of the child statement tree of the incoming node.
                    // Note that we do not need to check expressions since expressions cannot have statement children. If THIS node is a statement, it CANNOT be a child of any expressions.
                    ValidateCircular<VisualStatement>(context, stmt, VisualNodePropertyType.Statement);

#warning TODO: Remove other links if required

					// If we got here, validation has passed so make the link
					SetPropertyValue(targetProperty, new StatementReference(context, node));
					break;

				default:
					throw new VisualNodeLinkException($"Cannot link a VisualNode to the {prop.PropertyType.ToString()}-type property '{targetProperty}'.");
			}
        }

		/// <summary>
		/// Attempts to clears the between a node and the target property of this node. Optionally only clears the link for a specific Node ID.
		/// </summary>
		/// <param name="onlyIf">If a value for this parameter is provided, the reference will only be cleared if the ID of the target node of the reference matches this.</param>
		public void ClearLink(string targetProperty, Guid? onlyIf = null) {
			// Check the target property exists on this node
			if (!Properties.TryGetValue(targetProperty, out var prop))
				throw new ArgumentException($"Property could not be found on type '{GetType().Name}'.", nameof(targetProperty));

			// Check that this property is a NodeReference and the ID of the reference has a value (so that expressions with values instead aren't cleared) and, if requierd, the ID matches onlyIf
			if (GetPropertyValue(targetProperty) is INodeReference @ref && @ref.Id.HasValue && (onlyIf == null || @ref.Id == onlyIf))
				SetPropertyValue(targetProperty, prop.PropertyType == VisualNodePropertyType.Expression
					? Activator.CreateInstance(typeof(ExpressionReference<>).MakeGenericType(prop.PropertyDataType))
					: new StatementReference()
				);
		}

		/// <summary>
		/// Attempts to clear all references on this node. Optionally only clears references for a specific node ID.
		/// </summary>
		public void ClearAllLinks(Guid? onlyIf = null) {
			foreach (var prop in Properties.Where(x => x.Value.PropertyType == VisualNodePropertyType.Expression || x.Value.PropertyType == VisualNodePropertyType.Statement))
				ClearLink(prop.Key, onlyIf);
		}

        /// <summary>
        /// Performs a breadth-first validation to check the given link would not make a circular reference loop. If it does, throws a <see cref="VisualNodeLinkException"/>.
        /// </summary>
        /// <typeparam name="TNode">The type of node to check (e.g. <see cref="IVisualExpression"/> or <see cref="VisualStatement"/>)</typeparam>
        /// <param name="context">The running context for this nodes.</param>
        /// <param name="start">The initial node to start the search from.</param>
        /// <exception cref="VisualNodeLinkException">When a circular reference is detected.</exception>
        private void ValidateCircular<TNode>(VisualProgram context, TNode start, VisualNodePropertyType type) where TNode : IVisualNode {
            var itemsToCheck = new Queue<TNode>(new[] { start });

            // While there are still items in the queue
            while (itemsToCheck.TryDequeue(out var item)) {
                // Check if the next item matches the instance of this VisualNode
                if (Equals(item, this))
                    throw new VisualNodeLinkException("Cannot create link to a node that would cause a circular reference.");

                // For each child of the type we are searching for in the current item
                foreach (var child in item.GetPropertiesOfType(type))
                    // Check the property is a NodeReference (and not null), then attempt to resolve it (and defensively double-check it's the correct type).
                    if (item.GetPropertyValue(child.Name) is INodeReference @ref && @ref.ResolveNode(context) is TNode childItem)
                        // If so, add this to the queue to be checked
                        itemsToCheck.Enqueue(childItem);
            }
        }
    }



    /// <summary>
    /// An interface that VisualNodes must implement.
    /// </summary>
    /// <remarks>This is required in addition to the abstract class above because we need to provide a contract to the non-generic <see cref="IVisualExpression"/>.</remarks>
    public interface IVisualNode {

        /// <summary>The position of this node on the program canvas.</summary>
        Point Position { get; set; }

        /// <summary>Gets the Linq Expression for this node.</summary>
        /// <param name="context">The program context. Required for variables, resolving links, etc.</param>
        Expression CreateExpression(VisualProgram context);

        /// <summary>Gets all the properties of a specific <see cref="VisualNodePropertyType"/>, for example getting all statements.</summary>
        /// <param name="type">The type of property to filter by.</param>
        IEnumerable<PropDef> GetPropertiesOfType(VisualNodePropertyType type);

		/// <summary>Gets all the properties of a range of specific <see cref="VisualNodePropertyType"/>s, for example getting all statements AND all values.</summary>
		/// <param name="types">The types of property to filter by.</param>
		IEnumerable<PropDef> GetPropertiesOfTypes(params VisualNodePropertyType[] types) {
			var props = Enumerable.Empty<PropDef>();
			foreach (var type in types)
				props = props.Concat(GetPropertiesOfType(type));
			return props;
		}

        /// <summary>Gets the value of the property with the given name.</summary>
        object GetPropertyValue(string name);

        /// <summary>Sets the value of the property with the given name to the given value.</summary>
        /// <remarks>Note that no type checking is performed, only casting.</remarks>
        /// <exception cref="ArgumentException">If the property with the given name is not found.</exception>
        void SetPropertyValue(string name, object value);

        /// <summary>Attempts to link the given target <see cref="VisualNode"/> to the target property of this node.<para/>
        /// Will perform validation checks to ensure that the nodes can be linked, such as checking for expression/statement and expression type.</summary>
        void Link(VisualProgram context, string targetProperty, VisualNode node);
    }
}

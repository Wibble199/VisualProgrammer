using System;

namespace VisualProgrammer.Core {
       
    /// <summary>
    /// Attribute used to decorate properties on a <see cref="VisualNode" /> to allow them to be visible in the user interface.
    /// </summary>
    public class VisualNodePropertyAttribute : Attribute {

        /// <summary>
        /// A label for this property. If not set, the name of the property as defined in the code will be used.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// The order in which the property will appear in the auto-generated UI for the node.<para/>
        /// </summary>
        public int Order { get; set; }
    }

	public enum VisualNodePropertyType {
        /// <summary>This visual node property accepts a raw value (such as a string, int, etc).</summary>
        Value,
        /// <summary>This visual node property accepts a reference to an expression of a particular type.</summary>
        Expression,
        /// <summary>This visual node property accepts a reference to a statement.</summary>
        Statement,
		/// <summary>This visual node property accepts a reference to a variable.</summary>
		Variable
	}
}

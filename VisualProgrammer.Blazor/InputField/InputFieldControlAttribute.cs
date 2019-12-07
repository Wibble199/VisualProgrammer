using System;

namespace VisualProgrammer.Blazor.InputField {

    /// <summary>
    /// This attribute can be applied to Blazor controls to make them accessible by the 'Field' control.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InputFieldControlAttribute : Attribute {

        /// <summary>
        /// An array of types that this field can edit. Generally this is for allowing certain types on a generic control.
        /// </summary>
        public Type[] Types { get; set; }

        /// <summary>
        /// If a special name is provided, it will mark that this control is a specialized control for a particular purpose.
        /// An example of this could be a password input, which takes a string (like a normal text input does) but should not be used
        /// everywhere where a string is required. Metadata attributes on class properties will state whether that property should
        /// use field input with a special name.
        /// </summary>
        public string SpecialName { get; set; }

        /// <summary>
        /// The name of the property on the annotated class that gets or sets the value of the editor.
        /// Defaults to "Value".
        /// </summary>
        public string ValueParameterName { get; set; }

        /// <summary>
        /// The name of the property on the annotated class that is the <see cref="Microsoft.AspNetCore.Components.EventCallback{TValue}"/>
        /// which is called when the value of the editor changes. Defaults to "ValueChanged".
        /// </summary>
        public string ValueChangedParameterName { get; set; }

        /// <summary>
        /// If the annotated class is generic, and this value is true, the type that the control is being made for is passed as the generic argument.
        /// </summary>
        public bool PassGeneric { get; set; }

        /// <summary>
        /// Whether or not this control is a 'default' control that will allow being overriden by other input field controls of the same <see cref="Type"/>.
        /// This allows consumers of this library to replace the default controls with custom ones.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        public InputFieldControlAttribute(params Type[] types) {
            Types = types;
        }
    }
}

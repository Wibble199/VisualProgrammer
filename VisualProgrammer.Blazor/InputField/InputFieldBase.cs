using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace VisualProgrammer.Blazor.InputField {

    /// <summary>
    /// Base class for <see cref="InputField"/> and <see cref="InputFieldGeneric{TValue}"/> providing them a list of available controls that can be used.
    /// </summary>
    public abstract class InputFieldBase : ComponentBase {

        // A list of all available inner editor controls
        private static Dictionary<(Type dataType, string specialName), (Type controlType, InputFieldControlAttribute meta)> availableControls = new Dictionary<(Type, string), (Type, InputFieldControlAttribute)>();

        static InputFieldBase() {
            // Populate the available controls dictionary
            var detectedControls = AppDomain.CurrentDomain.GetAssemblies() // Get all the assemblies
                .SelectMany(assembly => {
                    try { return assembly.GetTypes(); } // Attempt to get all types in these assemblies
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); } // If there was an error loading some of them, return only the successful ones
                })
                .ToDictionary(type => type, type => (InputFieldControlAttribute)Attribute.GetCustomAttribute(type, typeof(InputFieldControlAttribute))) // Attempt to get any with InputFieldControlAttribute applied
                .Where(kvp => kvp.Value != null && typeof(IComponent).IsAssignableFrom(kvp.Key)); // Filter out any where the attribute is null (i.e. doesn't exist) and those that are Blazor components
                //.(x => (x.Value.Type, x.Value.SpecialName?.ToLower() ?? ""), x => (x.Key, x.Value));

            // Manually add to the dictionary so that we can properly handle the "IsDefault" flag.
            foreach (var control in detectedControls) {
                foreach (var type in control.Value.Types) {
                    var key = (type, control.Value.SpecialName?.ToLower() ?? "");
                    // Add to dictionary if there is no control with that type/name registered, OR if the registered type is a default and this isn't
                    if (!availableControls.ContainsKey(key) || (availableControls[key].meta.IsDefault && !control.Value.IsDefault))
                        availableControls[key] = (control.Key, control.Value);
                }
            }
        }

        /// <summary>Gets the editor type that will be used for the given data type.</summary>
        protected static (Type controlType, InputFieldControlAttribute meta)? GetControlFor(Type dataType, string specialName = "") {
            specialName = specialName.ToLower();
            return GetControlForExact(dataType, specialName) // Try to find this exact type in the controls list
                ?? (dataType.IsGenericType ? GetControlForExact(dataType.GetGenericTypeDefinition(), specialName) : null) // If nothing found, and the dataType is generic, seach for anything of that generic type (e.g. a control registered for Something<> can handle Something<string>)
                ?? (dataType.IsEnum ? GetControlFor(typeof(Enum), specialName) : null); // If the dataType is an enum, try to find the general Enum handler
        }

        /// <summary>Gets the control for this exact type. Should only be used from within <see cref="GetControlFor(Type, string)"/>.</summary>
        private static (Type controlType, InputFieldControlAttribute meta)? GetControlForExact(Type dataType, string specialName = "")
            => availableControls.ContainsKey((dataType, specialName)) ? availableControls[(dataType, specialName)]
             : availableControls.ContainsKey((dataType, "")) ? availableControls[(dataType, "")]
             : ((Type, InputFieldControlAttribute)?)null;
    }
}

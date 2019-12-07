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
        protected static (Type controlType, InputFieldControlAttribute meta)? GetControlFor(Type dataType, string specialName = "")
            => availableControls.ContainsKey((dataType, specialName.ToLower())) ? availableControls[(dataType, specialName)] // Try find a matching type and special name
             : availableControls.ContainsKey((dataType, "")) ? availableControls[(dataType, specialName)] // If not, try find the type with no special name
             : dataType.IsEnum && availableControls.ContainsKey((typeof(Enum), specialName.ToLower())) ? availableControls[(typeof(Enum), specialName.ToLower())] // If not, and the type is an enum, look for a generic enum control with that special name
             : dataType.IsEnum && availableControls.ContainsKey((typeof(Enum), "")) ? availableControls[(typeof(Enum), "")] // If not, and the type is an enum, look for a generic enum control with no special name
             : ((Type, InputFieldControlAttribute)?)null; // Found no matching control
    }
}

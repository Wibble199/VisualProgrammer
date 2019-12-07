using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Reflection;

namespace VisualProgrammer.Blazor.InputField {

    /// <summary>
    /// Represents a control that has a dynamic type and presents a relevant control to the user.
    /// <para/>
    /// This is the non-generic form of the InputField and should be used when the type of data to be edited is NOT known at compile-time. 
    /// If used with Blazor's @bind-Value attribute, it must be bound to an 'object' property.
    /// <para/>
    /// Use the <see cref="InputFieldGeneric{TValue}"/> component instead if the type is known at compile time.
    /// </summary>
    public sealed class InputFieldDynamic : InputFieldBase {

        [Parameter] public Type InputType { get; set; }
        [Parameter] public object Value { get; set; }
        [Parameter] public EventCallback<object> ValueChanged { get; set; }

        // Store some reflection info that we will use later, to save us performing more reflection
        private static MethodInfo createEventCallbackUnbound = typeof(InputFieldDynamic).GetMethod(nameof(CreateEventCallback), BindingFlags.NonPublic | BindingFlags.Instance); // using "nameof" means that the CreateEventCallback doesn't get a warning about being unused, so we protect agaisnt it being accidently cleaned up.

        protected override void BuildRenderTree(RenderTreeBuilder builder) {

            if (GetControlFor(InputType) is { } controlDef) {
                // If the control is generic and PassGeneric is true, create the closed generic
                var controlType = controlDef.controlType.IsGenericType && controlDef.meta.PassGeneric
                    ? controlDef.controlType.GetGenericTypeDefinition().MakeGenericType(InputType)
                    : controlDef.controlType;

                // If a control exists, create markup for this control, adding the relevant Value and ValueChanged attributes
                builder.OpenComponent(0, controlType);
                builder.AddAttribute(1, controlDef.meta.ValueParameterName ?? "Value", Value);
                builder.AddAttribute(2, controlDef.meta.ValueChangedParameterName ?? "ValueChanged", CreateDynamicEventCallback());
                builder.CloseComponent();

            } else if (InputType == null)
                // If the type to edit is null, show an error since there is no way of knowing which control to present
                builder.AddMarkupContent(0, "<div class='editor-type-error'>No datatype specified for this editor.</div>");
            else
                // If no control exists for this datatype, show a warning message
                builder.AddMarkupContent(0, $"<div class='editor-type-error'>Editor for the data type '{InputType.Name}' is unavailable.</div>");
        }

        /// <summary>
        /// Method that dynamically creates an <see cref="EventCallback{TValue}"/> whose TValue parameter matches that of the non-generic property
        /// <see cref="InputType"/>.
        /// This is required so that strictly typed EventCallbacks on input controls (e.g. EventCallback&lt;string&gt; on the TextBox control) can
        /// be dynamically set when the InputField does not know the TValue generic argument at compile time - only at runtime.
        /// </summary>
        private object CreateDynamicEventCallback() => createEventCallbackUnbound.MakeGenericMethod(InputType).Invoke(this, null);


        /// <summary>
        /// Method that creates an <see cref="EventCallback{TValue}"/> for the given type, which sets the <see cref="Value"/> to the incoming value
        /// and invokes this <see cref="InputField"/>'s <see cref="ValueChanged"/> callback with the new value too.
        /// </summary>
        /// <typeparam name="TValue">The type of value that the EventCallback should expect.</typeparam>
        private EventCallback<TValue> CreateEventCallback<TValue>() =>
            EventCallback.Factory.Create<TValue>(this, (TValue newValue) => {
                Value = newValue;
                ValueChanged.InvokeAsync(Value);
            });
    }
}

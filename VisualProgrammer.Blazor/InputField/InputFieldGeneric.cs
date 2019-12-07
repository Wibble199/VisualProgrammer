using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace VisualProgrammer.Blazor.InputField {

    /// <summary>
    /// Represents a control that has a particular (variable) type and presents a relevant control to the user.
    /// <para/>
    /// This is the generic form of the InputField for use when the type of data being edited IS known at compile time.
    /// Unlike it's non-generic counterpart, this component can be bound using @bind-Value to a property of the type being
    /// edited (e.g. int), you are not restricted to only being able to bind to object properties.
    /// <para/>
    /// Use the <see cref="InputFieldDynamic"/> component instead if the type is not known at compile time.
    /// </summary>
    public sealed class InputFieldGeneric<TValue> : InputFieldBase {
        [Parameter] public TValue Value { get; set; }
        [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder) {
            
            if (GetControlFor(typeof(TValue)) is { } controlDef) {
                // If the control is generic and PassGeneric is true, create the closed generic
                var controlType = controlDef.controlType.IsGenericType && controlDef.meta.PassGeneric
                    ? controlDef.controlType.GetGenericTypeDefinition().MakeGenericType(typeof(TValue))
                    : controlDef.controlType;

                // If a control exists, create markup for this control, adding the relevant Value and ValueChanged attributes
                builder.OpenComponent(0, controlDef.controlType);
                builder.AddAttribute(1, controlDef.meta.ValueParameterName ?? "Value", Value);
                builder.AddAttribute(2, controlDef.meta.ValueChangedParameterName ?? "ValueChanged", EventCallback.Factory.Create<TValue>(this, OnInputValueChanged));
                builder.CloseComponent();

            } else
                // If no control exists for this datatype, show a warning message
                builder.AddMarkupContent(0, $"<div class='editor-type-error'>Editor for the data type '{typeof(TValue).Name}' is unavailable.</div>");
        }

        private void OnInputValueChanged(TValue newValue) {
            Value = newValue;
            ValueChanged.InvokeAsync(Value);
        }
    }
}

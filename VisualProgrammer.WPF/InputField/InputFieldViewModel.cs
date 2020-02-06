using System;
using VisualProgrammer.WPF.ViewModels;

namespace VisualProgrammer.WPF.InputField {

	/// <summary>
	/// Very simple view-model to be passed to the DataTemplates used by the <see cref="InputFieldDynamic"/>.
	/// </summary>
	public class InputFieldViewModel : ViewModelBase<InputFieldDynamic> {

		public InputFieldViewModel(InputFieldDynamic model) : base(model) {
			// When the 'Value' in the parent control changes, we need to raise the change event here too
			model.ValueChanged += (sender, e) => Notify(nameof(Value));
		}

		/// <summary>
		/// Readonly property that indicates the type of value requested by the field.
		/// </summary>
		public Type InputType => model.InputType;

		/// <summary>
		/// Proxy for getting or setting the value that is being edited by this field.
		/// </summary>
		public object Value {
			get => model.Value;
			set => SetAndNotify(_ => model.Value, value);
		}
	}
}

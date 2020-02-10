using System;
using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF.InputField {

	public class InputFieldDynamic : Control {

		static InputFieldDynamic() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(InputFieldDynamic), new FrameworkPropertyMetadata(typeof(InputFieldDynamic)));
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			if (GetTemplateChild("PART_ContentControl") is ContentControl cc)
				cc.DataContext = new InputFieldViewModel(this);
		}

		#region InputType DependencyProperty
		public Type InputType {
			get => (Type)GetValue(InputTypeProperty);
			set => SetValue(InputTypeProperty, value);
		}

		public static readonly DependencyProperty InputTypeProperty =
			DependencyProperty.Register("InputType", typeof(Type), typeof(InputFieldDynamic), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

		#region Value DependencyProperty
		public object Value {
			get => GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(object), typeof(InputFieldDynamic), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValuePropertyChanged));

		// Need to tell the view-model that the value has changed so that anything bound to that will update if the value is changed externally.
		private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((InputFieldDynamic)d).ValueChanged?.Invoke(d, EventArgs.Empty);
		public event EventHandler ValueChanged;
		#endregion
	}


	/// <summary>
	/// DataTemplateSelector that tries to find a DataTemplate that is keyed with the type specified in the container's view-model's InputType.
	/// </summary>
	public class InputFieldDataTemplateSelector : DataTemplateSelector {

		// Key to the fallback template to be displayed if an editor cannot be found
		private static readonly ComponentResourceKey errorKey = new ComponentResourceKey(typeof(InputFieldDynamic), "MissingInputFieldDataTemplate");

		public override DataTemplate SelectTemplate(object item, DependencyObject container) {
			if (!(container is ContentPresenter presenter)) return null;
			if (item is InputFieldViewModel vm) {
				// Func to return a DataTemplate for the type (or null if not found)
				DataTemplate TemplateFor(Type type) => presenter.TryFindResource(new InputFieldTemplateKey(type)) as DataTemplate;

				return TemplateFor(vm.InputType) // First, try to find a template for this exact type
					?? TemplateFor(typeof(Enum)); // Next, if the type is an enum (and no specialised control is registered as above), then use the general enum editor
			}
			return presenter.FindResource(errorKey) as DataTemplate;
		}
	}


	/// <summary>
	/// A specialised key that can be used to specify a DataTemplate to use by the InputField for the target type.
	/// </summary>
	public class InputFieldTemplateKey : ComponentResourceKey {

		/// <summary>
		/// Creates a new <see cref="InputFieldTemplateKey"/> that provides a data template for the given type.
		/// </summary>
		/// <param name="targetType">The type of data that this editor is for.</param>
		public InputFieldTemplateKey(Type targetType)
			: base(typeof(InputFieldTemplateKey), targetType) { }
	}
}

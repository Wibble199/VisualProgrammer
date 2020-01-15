using System;
using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF.InputField {

	public class InputFieldDynamic : Control {

		static InputFieldDynamic() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(InputFieldDynamic), new FrameworkPropertyMetadata(typeof(InputFieldDynamic)));
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
			DependencyProperty.Register("Value", typeof(object), typeof(InputFieldDynamic), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion
	}
}

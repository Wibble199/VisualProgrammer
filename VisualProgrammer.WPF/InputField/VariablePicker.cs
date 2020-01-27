using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.ViewModels;

namespace VisualProgrammer.WPF.InputField {

	/// <summary>
	/// Control that provides a specialized variant of a <see cref="ComboBox"/> that displays variables of the target type based on the program view-model context
	/// as given by <see cref="VisualProgramViewModelAttachedProperty.VisualProgramModelProperty"/>.<para/>
	/// </summary>
	public class VariablePicker : Control {

		static VariablePicker() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VariablePicker), new FrameworkPropertyMetadata(typeof(VariablePicker)));
		}

		#region VariableType DependencyProperty
		/// <summary>
		/// The type of variable to filter the variable list on.
		/// </summary>
		public Type VariableType {
			get => (Type)GetValue(VariableTypeProperty);
			set => SetValue(VariableTypeProperty, value);
		}

		public static readonly DependencyProperty VariableTypeProperty =
			DependencyProperty.Register("VariableType", typeof(Type), typeof(VariablePicker), new PropertyMetadata(null));
		#endregion

		#region SelectedVariable DependencyProperty
		/// <summary>
		/// Gets or sets the variable that is selected by this picker. Expects and returns a <see cref="VariableReference{TVar}"/>.
		/// </summary>
		public IVariableReference SelectedVariable {
			get => (IVariableReference)GetValue(SelectedVariableProperty);
			set => SetValue(SelectedVariableProperty, value);
		}

		public static readonly DependencyProperty SelectedVariableProperty =
			DependencyProperty.Register("SelectedVariable", typeof(IVariableReference), typeof(VariablePicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		#endregion
	}


	/// <summary>
	/// Converter that takes a <see cref="ObservableCollection{T}"/> of <see cref="VariableDefinitionViewModel"/> and a <see cref="Type"/>
	/// and returns a filtered list of <see cref="VariableDefinitionViewModel"/> that are of the target type.
	/// </summary>
	public class VariableListFilterConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
			values[0] is ObservableCollection<VariableDefinitionViewModel> collection && values[1] is Type type
				? collection.Where(x => x.Type == type)
				: null;

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}


	/// <summary>
	/// A converter that converts a <see cref="VariableReference{TVar}"/> into it's variable ID string and vice-versa.
	/// </summary>
	public class VariableReferenceConverter : IMultiValueConverter {

		private Type variableType;

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (values[1] is Type t) variableType = t; // Store the variable type so that the converter knows what to convert it to during ConvertBack.
			return (values[0] as IVariableReference)?.Name; // The actual conversion
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			if (variableType == null)
				throw new Exception("Variable type not yet set. Cannot create a VariableReference of unknown type");
			return new[] { VariableReference.Create(variableType, (value as string) ?? ""), Binding.DoNothing }; // Send DoNothing as the binding that was used to get the type so that we do not set it
		}
	}
}

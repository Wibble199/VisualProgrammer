using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VisualProgrammer.WPF.ViewModels;
using Expression = System.Linq.Expressions.Expression;

namespace VisualProgrammer.WPF.InputField {

	#region Generic Bases
	/// <summary>
	/// Base class for the numeric steppers that has a UI template.
	/// </summary>
	public abstract class NumericStepper : Control {
		static NumericStepper() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericStepper), new FrameworkPropertyMetadata(typeof(NumericStepper)));
		}
	}

	/// <summary>
	/// A generic base class for the numeric steppers that implements DependencyProperties for Value and Step.<para/>
	/// Also generates compiled lambdas that are bound to the type <typeparamref name="T"/> and increment or decrement the <see cref="Value"/>.
	/// </summary>
	/// <typeparam name="T">The type of number being represented by this stepper.</typeparam>
	public abstract class NumericStepper<T> : NumericStepper {

		protected static readonly Action<NumericStepper<T>> increment;
		protected static readonly Action<NumericStepper<T>> decrement;

		static NumericStepper() {
			// Generic a compiled lambda for incrementing/decrementing the value.
			var selfParameter = Expression.Parameter(typeof(NumericStepper<T>));
			var valueProp = Expression.PropertyOrField(selfParameter, nameof(Value));
			var stepProp = Expression.PropertyOrField(selfParameter, nameof(Step));
			increment = Expression.Lambda<Action<NumericStepper<T>>>(Expression.AddAssignChecked(valueProp, stepProp), selfParameter).Compile();
			decrement = Expression.Lambda<Action<NumericStepper<T>>>(Expression.SubtractAssignChecked(valueProp, stepProp), selfParameter).Compile();
		}

		protected NumericStepper() {
			// Create commands for the View that will execute the compiled lambdas.
			IncrementCommand = new DelegateCommand(() => increment(this));
			DecrementCommand = new DelegateCommand(() => decrement(this));
		}

		#region Commands
		/// <summary>Command to increment the <see cref="Value"/> by <see cref="Step"/>.</summary>
		public ICommand IncrementCommand { get; }
		/// <summary>Command to decrement the <see cref="Value"/> by <see cref="Step"/>.</summary>
		public ICommand DecrementCommand { get; }
		#endregion

		#region Value DependencyProperty
		/// <summary>
		/// The current value of this numeric stepper.
		/// </summary>
		public T Value {
			get => (T)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("Value", typeof(T), typeof(NumericStepper<T>), new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceValueProperty));

		private static object CoerceValueProperty(DependencyObject d, object baseValue) => baseValue is T ? baseValue : Convert.ChangeType(baseValue, typeof(T));
		#endregion

		#region Step DependencyProperty
		/// <summary>
		/// The amount the <see cref="Value"/> is changed by when pressing the step up/down buttons.
		/// </summary>
		public T Step {
			get => (T)GetValue(StepProperty);
			set => SetValue(StepProperty, value);
		}

		// Note that this Property should have it's metadata overriden in derived classes to have a valid default value.
		// Unlike with Value above, we cannot use default as this will likely be zero, and there is no point having a step size of zero.
		public static readonly DependencyProperty StepProperty =
			DependencyProperty.Register("Step", typeof(T), typeof(NumericStepper<T>), new PropertyMetadata(null));
		#endregion
	}
	#endregion


	#region Concrete Implementations
	public class IntegerStepper : NumericStepper<int> {
		static IntegerStepper() => StepProperty.OverrideMetadata(typeof(IntegerStepper), new PropertyMetadata(1));
	}

	public class DoubleStepper : NumericStepper<double> {
		static DoubleStepper() => StepProperty.OverrideMetadata(typeof(DoubleStepper), new PropertyMetadata(1d));
	}
	#endregion
}

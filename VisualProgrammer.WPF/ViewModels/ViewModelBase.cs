using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace VisualProgrammer.WPF.ViewModels {

	/// <summary>
	/// A base view-model that provides some helpful methods for other models.
	/// </summary>
	/// <typeparam name="TModel">The base model type that this view-model is for.</typeparam>
	public abstract class ViewModelBase<TModel> : INotifyPropertyChanged {

		/// <summary>The wrapped model of this view-model.</summary>
		protected internal readonly TModel model;

		public event PropertyChangedEventHandler PropertyChanged;

		protected ViewModelBase(TModel model) {
			this.model = model;
		}

		/// <summary>
		/// Sets the referenced field to be the specified value and raises <see cref="PropertyChanged"/> IF the current value of the field does not equal the new value.
		/// </summary>
		/// <param name="propertyName">The name of the property passed to the event. Note that this is annotated with <see cref="CallerMemberNameAttribute"/>.</param>
		protected void SetAndNotify<T>(ref T target, T newValue, [CallerMemberName] string propertyName = null) {
			if (!EqualityComparer<T>.Default.Equals(target, newValue)) {
				target = newValue;
				Notify(propertyName);
			}
		}

		/// <summary>
        /// Sets the target property on the model to the given value. If it is different, raises a <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="property">An expression that resolves to the property on the backing model.</param>
        protected void SetAndNotify<T>(Expression<Func<TModel, T>> property, T value, [CallerMemberName] string propertyName = null) {
            // Run the property selector to get the current value and compare that to the incoming value
            // If they are different, we need to update the property at this location
            if (!EqualityComparer<T>.Default.Equals(property.Compile()(model))) {
				switch (property.Body) {
					// Check the property selector is a member expression
					case MemberExpression expr:
						// Make a getter that returns the parent object since we need to pass this to SetValue
						// E.G. if we had (model) => model.Foo.Bar.A, then expr.Expression points to model.Foo.Bar
						// We also need to pass the model parameter in so that the property can be selected properly
						var parent = Expression.Lambda<Func<TModel, object>>(expr.Expression, property.Parameters).Compile();

						// Actually set the property on the target "parent" instance
						if (expr.Member is PropertyInfo pi)
							pi.SetValue(parent(model), value);
						else if (expr.Member is FieldInfo fi)
							fi.SetValue(parent(model), value);
						break;

					// Check if the property selector is a method called get_Item (this is what's created when accessing indexed things such as lists by myList[2])
					case MethodCallExpression expr when expr.Object != null && expr.Method.IsSpecialName && expr.Method.Name == "get_Item":
						// Try find a matching set_Item method. Making an assumption here that it will have the same parameters (with the addition of the value at the end)
						if (expr.Method.DeclaringType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Public) is { } setter) {
							// Add the new value to the end of the get_Item's argument expression list (this means that the indexes are preserved properly)
							var args = expr.Arguments.Concat(new[] { Expression.Constant(value) });
							// Create an action which takes the model as a parameter and calls the set_Item method with the arguments specified above
							var execute = Expression.Lambda<Action<TModel>>(Expression.Call(expr.Object, setter, args), property.Parameters).Compile();
							// Actually execute the compiled setter method
							execute(model);

						} else
							throw new ArgumentException("Property selector uses a 'get_Item' method which does not have a corresponding 'set_Item' method.", nameof(property));
						break;

					default:
						throw new ArgumentException("Property selector does not have a member expression as the body.", nameof(property));
				}

				// Notify that it's changed
				Notify(propertyName);
            }
        }

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event with the given property name.
		/// </summary>
		/// <param name="propertyName">The name of the property passed to the event. Note that this is annotated with <see cref="CallerMemberNameAttribute"/>.</param>
		protected void Notify([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}

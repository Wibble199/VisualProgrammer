using System;
using System.ComponentModel;
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
			if (!Equals(target, newValue)) {
				target = newValue;
				Notify(propertyName);
			}
		}

		/// <summary>
		/// Executes the target setter action with the given value and raises <see cref="PropertyChanged"/>.
		/// </summary>
		/// <param name="propertyName">The name of the property passed to the event. Note that this is annotated with <see cref="CallerMemberNameAttribute"/>.</param>
		protected void SetAndNotify<T>(Action<T> setter, T newValue, [CallerMemberName] string propertyName = null) {
			setter(newValue);
			Notify(propertyName);
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

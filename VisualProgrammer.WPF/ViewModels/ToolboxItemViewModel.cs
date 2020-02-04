using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF.ViewModels {

	/// <summary>
	/// A model for the items displayed in the toolbox.
	/// </summary>
	public sealed class ToolboxItemViewModel : ViewModelBase<Type> {

		private readonly VisualProgramViewModel parent;
		internal ObservableCollection<Type> selectedGenericTypes = new ObservableCollection<Type>();

		public ToolboxItemViewModel(VisualProgramViewModel parent, Type type) : base(type) {
			this.parent = parent;

			// Create a collection with as many types as there are generic arguments in the target IVisualNode type
			for (var i = 0; i < type.GetGenericArguments().Length; i++) {
				selectedGenericTypes.Add(typeof(string));
				SelectedGenericTypesViewModels.Add(new ToolboxItemGenericArgumentViewModel(this, i));
			}
			selectedGenericTypes.CollectionChanged += (sender, e) => Notify(nameof(TemplateInstance));

			// TODO: Add support for restricting allowed generic types based on attributes or type constraints
		}

		/// <summary>
		/// Gets the ViewModel instance that can be given to a <see cref="VisualNodePresenter"/> to show a template of the node.
		/// </summary>
		public VisualNodeViewModel TemplateInstance => new VisualNodeViewModel(
			(VisualNode)Activator.CreateInstance(model.ContainsGenericParameters ? model.MakeGenericType(selectedGenericTypes.ToArray()) : model),
			Guid.Empty
		);

		/// <summary>
		/// A collection of the ViewModels for generic arguments that the user has selected.
		/// </summary>
		public List<ToolboxItemGenericArgumentViewModel> SelectedGenericTypesViewModels { get; } = new List<ToolboxItemGenericArgumentViewModel>();

		/// <summary>
		/// Creates a new node on the parent program view-model and starts drag behaviourfor it.
		/// </summary>
		public void CreateNode(object sender, MouseEventArgs e) {
			var nodeVm = parent.CreateNode(model, selectedGenericTypes.ToArray());
			parent.DragBehaviour = new VisualNodePresenterDragBehaviour(nodeVm, e.GetPosition((IInputElement)sender));
		}
	}

	/// <summary>
	/// View-Model to allow two-way binding for the generic types selected by the user.
	/// </summary>
	public sealed class ToolboxItemGenericArgumentViewModel : ViewModelBase<int> {
		private readonly ToolboxItemViewModel parent;

		public ToolboxItemGenericArgumentViewModel(ToolboxItemViewModel parent, int index) : base(index) {
			this.parent = parent;
		}

		/// <summary>
		/// Gets or sets the selected type for this generic argument.
		/// </summary>
		public Type Value {
			get => parent.selectedGenericTypes[model];
			set => SetAndNotify(idx => parent.selectedGenericTypes[idx], value);
		}

		/// <summary>
		/// A list of all types that are allowed as this generic parameter.
		/// </summary>
		public IEnumerable<Type> AllowedTypes => new[] { typeof(string), typeof(int), typeof(double), typeof(bool) };
	}
}

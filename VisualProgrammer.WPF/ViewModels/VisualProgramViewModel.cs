using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.Util;

namespace VisualProgrammer.WPF.ViewModels {

	/// <summary>
	/// View-model wrapper for a <see cref="VisualProgram"/>.
	/// </summary>
	public sealed class VisualProgramViewModel : ViewModelBase<VisualProgram> {

		internal VisualProgramViewModel(VisualProgram model) : base(model) {
			// Initialise nested models
			nodeIdViewModelMap = model.Nodes.ToDictionary(kvp => kvp.Id, kvp => new VisualNodeViewModel(kvp));
			Nodes = new ObservableCollection<VisualNodeViewModel>(nodeIdViewModelMap.Values);
			AvailableNodes = model.Environment.AvailableNodeTypes.Select(t => new ToolboxItemViewModel(this, t)).ToList();

			Variables = new ObservableCollection<VariableDefinitionViewModel>(model.Variables.Select(var => new VariableDefinitionViewModel(var)));
			Variables.CollectionChanged += Variables_CollectionChanged;

			// Initialise commands
			// TODO
		}

		/// <summary>The drag behaviour currently being executed. This determines what happens when the user moves or releases their mouse.</summary>
		internal ICanvasDragBehaviour DragBehaviour { get; set; }

		#region Nodes
		// Dictionary with holds the VisualNodeViewModels by their Node IDs. This allows easy finding of a specific model, rather than needing to loop over Nodes.
		private readonly Dictionary<Guid, VisualNodeViewModel> nodeIdViewModelMap;

		/// <summary>A collection of all nodes that currently exist on the Visual Program's canvas.</summary>
		public ObservableCollection<VisualNodeViewModel> Nodes { get; }

		/// <summary>A list of all available node types that can be added to this program.</summary>
		public IEnumerable<ToolboxItemViewModel> AvailableNodes { get; }

		public VisualNodeViewModel CreateNode(Type nodeType, params Type[] genericTypes) {
			var node = model.Nodes.Create(nodeType, genericTypes);
			var vm = new VisualNodeViewModel(node);
			nodeIdViewModelMap.Add(node.Id, vm);
			Nodes.Add(vm);
			return vm;
		}

		public void RemoveNode(Guid nodeId) {
			model.Nodes.Remove(nodeId);
			Nodes.Remove(nodeIdViewModelMap[nodeId]);
			nodeIdViewModelMap.Remove(nodeId);
		}
		#endregion

		#region Variables
		/// <summary>A collection of variables currently defined on the VisualProgram model.</summary>
		public ObservableCollection<VariableDefinitionViewModel> Variables { get; }

		/// <summary>
		/// Handler that executes when the Variable collection is changed and propogates the changes to the model.
		/// </summary>
		private void Variables_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (var item in e.OldItems.Cast<VariableDefinitionViewModel>())
					model.Variables.Remove(item.ID);
			}
			Notify(nameof(Variables));
		}
		#endregion
	}
}

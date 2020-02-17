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

			EntryDisplayNames = new ReadOnlyDictionary<string, string>(model.Environment.EntryDefinitions.ToDictionary(e => e.Key, e => e.Value.Name));
			AvailableEntries = model.Environment.EntryDefinitions.Values.Select(e => new ToolboxEntryViewModel(this, e)).ToList();

			Variables = new ObservableCollection<VariableDefinitionViewModel>(model.Variables.Select(var => {
				var varVm = new VariableDefinitionViewModel(var, model.Environment);
				varVm.RequestDelete += DeleteVariable;
				return varVm;
			}));

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

		/// <summary>A map of entry IDs onto their display names.</summary>
		public IReadOnlyDictionary<string, string> EntryDisplayNames { get; }

		/// <summary>A list of all available entries that can be added to this program.</summary>
		public IEnumerable<ToolboxEntryViewModel> AvailableEntries { get; }

		public VisualNodeViewModel CreateNode(Type nodeType, params Type[] genericTypes) => CreateNode(model.Nodes.Create(nodeType, genericTypes));
		public VisualNodeViewModel CreateNode(EntryDefinition entryDefinition) => CreateNode(model.Nodes.Create(entryDefinition));
		private VisualNodeViewModel CreateNode(VisualNode node) {
			var vm = new VisualNodeViewModel(node);
			nodeIdViewModelMap.Add(node.Id, vm);
			Nodes.Add(vm);
			Notify(nameof(Nodes));
			return vm;
		}

		public void RemoveNode(Guid nodeId) {
			model.Nodes.Remove(nodeId);
			Nodes.Remove(nodeIdViewModelMap[nodeId]);
			nodeIdViewModelMap.Remove(nodeId);
			Notify(nameof(Nodes));
		}
		#endregion

		#region Variables
		/// <summary>A collection of variables currently defined on the VisualProgram model.</summary>
		public ObservableCollection<VariableDefinitionViewModel> Variables { get; }

		/// <summary>
		/// Handler for when a VariableDefinitionViewModel requests to be deleted.
		/// </summary>
		private void DeleteVariable(object sender, EventArgs e) {
			if (sender is VariableDefinitionViewModel varVM && model.Variables.Remove(varVM.ID)) {
				Variables.Remove(varVM);
				Notify(nameof(Variables));
			}
		}
		#endregion
	}
}

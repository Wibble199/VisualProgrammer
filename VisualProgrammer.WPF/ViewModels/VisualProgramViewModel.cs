using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			nodeIdViewModelMap = model.Nodes.ToDictionary(kvp => kvp.Key, kvp => new VisualNodeViewModel(kvp.Value, kvp.Key));
			Nodes = new ObservableCollection<VisualNodeViewModel>(nodeIdViewModelMap.Values);

			variableIdViewModelMap = model.variableDefinitions.ToDictionary(kvp => kvp.Key, kvp => new VariableDefinitionViewModel(kvp.Value, kvp.Key));
			Variables = new ObservableCollection<VariableDefinitionViewModel>(variableIdViewModelMap.Values);

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

		public Guid CreateNode(Type nodeType, params Type[] genericTypes) {
			var guid = model.CreateNode(nodeType, genericTypes);
			var vm = new VisualNodeViewModel(model.Nodes[guid], guid);
			nodeIdViewModelMap.Add(guid, vm);
			Nodes.Add(vm);
			return guid;
		}

		public void RemoveNode(Guid nodeId) {
			model.RemoveNode(nodeId);
			Nodes.Remove(nodeIdViewModelMap[nodeId]);
			nodeIdViewModelMap.Remove(nodeId);
		}
		#endregion

		#region Variables
		// Dictionary with holds the VariableDefinitionViewModels by their Varaible IDs.
		private readonly Dictionary<string, VariableDefinitionViewModel> variableIdViewModelMap;

		/// <summary>A collection of variables currently defined on the VisualProgram model.</summary>
		public ObservableCollection<VariableDefinitionViewModel> Variables { get; }
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualProgrammer.Core.Environment {

	/// <summary>
	/// A fluent builder that helps construct an execution environment for the VisualProgram.
	/// </summary>
	public sealed class VisualProgramEnvironmentBuilder {

		private static readonly VisualNodeConfigurator defaultNodes = new VisualNodeConfigurator().IncludeDefault();
		private static readonly DataTypesConfigurator defaultDataTypes = new DataTypesConfigurator().AddDefault();

		private VisualNodeConfigurator? nodeConfigurator;
		private VisualEntryConfigurator? entryConfigurator;
		private DataTypesConfigurator? dataTypesConfigurator;
		private LockedVariableConfigurator? lockedVarConfigurator;

		internal VisualProgramEnvironmentBuilder() { }

		/// <summary>
		/// Configures the nodes that are available to be added to the program.
		/// </summary>
		/// <param name="config">A function that is passed a configurator that can be used to configure the available nodes.</param>
		/// <exception cref="InvalidOperationException">When the nodes have already been configured.</exception>
		public VisualProgramEnvironmentBuilder ConfigureNodes(Action<VisualNodeConfigurator> config) {
			if (nodeConfigurator != null) throw new InvalidOperationException("Cannot configure the available nodes. They have already been configured.");
			nodeConfigurator = new VisualNodeConfigurator();
			config(nodeConfigurator);
			return this;
		}

		/// <summary>
		/// Configures the entries that are available to the program.
		/// </summary>
		/// <param name="config">A function that is passed a configurator that can be used to configure the entries.</param>
		/// <exception cref="InvalidOperationException">When the entries have already been configured.</exception>
		public VisualProgramEnvironmentBuilder ConfigureEntries(Action<VisualEntryConfigurator> config) {
			if (entryConfigurator != null) throw new InvalidOperationException("Cannot configure the node entries. They have already been configured.");
			entryConfigurator = new VisualEntryConfigurator();
			config(entryConfigurator);
			return this;
		}

		/// <summary>
		/// Configures the data types that are available. This will determine the varaible types that can be created by the user and which
		/// types can be used when adding generic nodes.
		/// </summary>
		/// <param name="config">A function that is passed a configurator that can be used to configure the types.</param>
		/// <exception cref="InvalidOperationException">When the data types have already been configured.</exception>
		public VisualProgramEnvironmentBuilder ConfigureDataTypes(Action<DataTypesConfigurator> config) {
			if (dataTypesConfigurator != null) throw new InvalidOperationException("Cannot configure the data types. They have already been configured.");
			dataTypesConfigurator = new DataTypesConfigurator();
			config(dataTypesConfigurator);
			return this;
		}

		/// <summary>
		/// Configures the variables that will always be present in the program and which the user cannot delete.
		/// Useful for when an interface is being implemented by the program and you require certain properties to be present.
		/// </summary>
		/// <param name="config">A function that is passed a configurator that can be used to configure the locked variables.</param>
		/// <exception cref="InvalidOperationException">When the locked variables have already been configured.</exception>
		public VisualProgramEnvironmentBuilder ConfigureLockedVariables(Action<LockedVariableConfigurator> config) {
			if (dataTypesConfigurator != null) throw new InvalidOperationException("Cannot configure the locked variables. They have already been configured.");
			lockedVarConfigurator = new LockedVariableConfigurator();
			config(lockedVarConfigurator);
			return this;
		}

		/// <summary>
		/// Constructs an environment configuration to be passed to a VisualProgram.
		/// </summary>
		internal VisualProgramEnvironment Build() => new VisualProgramEnvironment {
			AvailableNodeTypes = (nodeConfigurator ?? defaultNodes).Types,
			DataTypes = (dataTypesConfigurator ?? defaultDataTypes).types,
			EntryDefinitions = entryConfigurator?.entries ?? new Dictionary<string, EntryDefinition>(StringComparer.OrdinalIgnoreCase),
			LockedVariables = lockedVarConfigurator?.variables.Values ?? Enumerable.Empty<Variable>()
		};
	}
}

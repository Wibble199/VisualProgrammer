using System;
using System.Collections.Generic;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core.Environment {

	/// <summary>
	/// Provides a fluent API for configuring the entries that are available for a program.
	/// </summary>
	public sealed class VisualEntryConfigurator {

		internal Dictionary<string, EntryDefinition> entries;

		internal VisualEntryConfigurator() { }

		/// <summary>
		/// Registers a new entry with the given ID, name and parameters.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the given ID or name is null or empty.</exception>
		/// <exception cref="ArgumentException">If an entry with this ID has already been added.</exception>
		public VisualEntryConfigurator Add(string id, string name, Action<VisualEntryParameterConfigurator> parameters) {
			if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "ID must be non-null and non-empty.");
			if (entries.ContainsKey(id)) throw new ArgumentException($"An entry with this ID ('{id}') has already been registered.", nameof(id));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name), "Name must be non-null and non-empty.");

			var paramConfig = new VisualEntryParameterConfigurator();
			parameters(paramConfig);
			var ed = new EntryDefinition { Name = name, Parameters = paramConfig.parameters };
			entries.Add(id, ed);
			return this;
		}

		/// <summary>
		/// Registers a new parameterless entry with the given ID and name.
		/// </summary>
		public VisualEntryConfigurator Add(string id, string name) => Add(id, name, _ => { });
	}


	/// <summary>
	/// Fluent API for configuring the parameters for an entry definition.
	/// </summary>
	public sealed class VisualEntryParameterConfigurator {

		internal readonly IndexedDictionary<string, Type> parameters;

		internal VisualEntryParameterConfigurator() { }

		/// <summary>
		/// Appends a new parameter with the given name and type.<para/>
		/// Note that the parameters are ordered in the same order they are added.
		/// </summary>
		/// <param name="name">The (unique) name for this parameter.</param>
		/// <exception cref="ArgumentNullException">If the given parameter name is null or empty.</exception>
		/// <exception cref="ArgumentException">If a parameter with this name has already been added.</exception>
		public VisualEntryParameterConfigurator WithParameter(string name, Type type) {
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name), "Parameter name must not be null or empty.");
			parameters.Add(name, type);
			return this;
		}
	}
}

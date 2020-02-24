using System;
using System.Collections.Generic;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core.Environment {

	/// <summary>
	/// Provides a fluent API for configuring the entries that are available for a program.
	/// </summary>
	public sealed class VisualEntryConfigurator {

		internal readonly Dictionary<string, EntryDefinition> entries = new Dictionary<string, EntryDefinition>(StringComparer.OrdinalIgnoreCase);

		internal VisualEntryConfigurator() { }

		/// <summary>
		/// Registers a new entry with the given ID, name and parameters.
		/// </summary>
		/// <exception cref="ArgumentNullException">If the given ID or name is null or empty.</exception>
		/// <exception cref="ArgumentException">If an entry with this ID has already been added.</exception>
		public VisualEntryConfigurator Add(string id, string name, Action<VisualEntryParameterConfigurator> parameters) {
			if (entries.ContainsKey(id)) throw new ArgumentException($"An entry with this ID ('{id}') has already been registered.", nameof(id));

			var paramConfig = new VisualEntryParameterConfigurator();
			parameters(paramConfig);
			var ed = new EntryDefinition(id, name, paramConfig.parameters);
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

		internal readonly IndexedDictionary<string, Type> parameters = new IndexedDictionary<string, Type>();

		internal VisualEntryParameterConfigurator() { }

		/// <summary>
		/// Appends a new parameter with the given name and type.<para/>
		/// Note that the parameters are ordered in the same order they are added.
		/// </summary>
		/// <param name="name">The (unique) name for this parameter.</param>
		/// <exception cref="ArgumentNullException">If the given parameter name is null or empty.</exception>
		/// <exception cref="ArgumentException">If a parameter with this name has already been added.</exception>
		/// <exception cref="NotSupportedException">If this method is called when there are 15 parameters already defined.</exception>
		public VisualEntryParameterConfigurator WithParameter(string name, Type type) {
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name), "Parameter name must not be null or empty.");

			// Since the compiler attempts to get a closed generic action type from the function definition from the function definition, it is limited by the max-sized generic action, which has 16 generic args
			// However, we also need to pass the compiled instance base context in as the first parameter, leaving 15 parameters left for the entry. Hence this limit.
			if (parameters.Count >= 15) throw new NotSupportedException("Cannot pass more than 15 parameters to a single entry.");

			parameters.Add(name, type);
			return this;
		}

		/// <summary>
		/// Appends a new parameter with the given name and type.<para/>
		/// Note that the parameters are ordered in the same order they are added.
		/// </summary>
		/// <param name="name">The (unique) name for this parameter.</param>
		/// <exception cref="ArgumentNullException">If the given parameter name is null or empty.</exception>
		/// <exception cref="ArgumentException">If a parameter with this name has already been added.</exception>
		/// <exception cref="NotSupportedException">If this method is called when there are 15 parameters already defined.</exception>
		public VisualEntryParameterConfigurator WithParameter<T>(string name) => WithParameter(name, typeof(T));
	}
}

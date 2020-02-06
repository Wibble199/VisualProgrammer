using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VisualProgrammer.Core.Environment {

	/// <summary>
	/// A class that defines the environment for a <see cref="VisualProgram"/>. The environment determines factors
	/// such as what nodes and types are available for use in the program.
	/// <para />
	/// Use <see cref="VisualProgramEnvironmentBuilder"/> to create an environment.
	/// </summary>
	public sealed class VisualProgramEnvironment {

		[SuppressMessage("", "CS8618", Justification = "These properties are set by the VisualProgramEnvironmentBuilder.")]
		internal VisualProgramEnvironment() { }

		/// <summary>Gets the available node types that are allowed to be added to a program using this environment.</summary>
		public IEnumerable<Type> AvailableNodeTypes { get; internal set; }

		/// <summary>Gets the data types that are supported by this environment. Will determine what variable types and
		/// generic node types are available.</summary>
		public IEnumerable<Type> DataTypes { get; internal set; }

		/// <summary>Gets the entry definitions that are recognised by a program using this environment.</summary>
		public Dictionary<string, EntryDefinition> EntryDefinitions { get; internal set; }
	}
}

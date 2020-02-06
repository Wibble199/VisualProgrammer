﻿using System;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Class that defines a single entry point for a VisualProgram.
	/// </summary>
	public sealed class EntryDefinition {
		/// <summary>
		/// The user-friendly display name of the definition.
		/// </summary>
		public string Name { get; set; } = "";

        /// <summary>
        /// Specifies the parameters that will be passed to this entry. This will define the signature of the compiled delegate generated by this VisualEntry.<para/>
        /// Note that it IS safe to re-order parameters between versions without breaking existing programs (since the lambda is recompiled at runtime), however it IS
        /// NOT safe to rename or change the type of existing parameters (because this may break existing parameter maps in the VisualEntry instances). It is also safe
        /// to add new parameters without affecting existing programs.
        /// </summary>
        public IndexedDictionary<string, Type> Parameters { get; set; } = new IndexedDictionary<string, Type>();
    }
}
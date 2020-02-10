using System;
using System.Linq;
using VisualProgrammer.Core.Compilation;
using VisualProgrammer.Core.Environment;

namespace VisualProgrammer.Core {

	/// <summary>
	/// The class that is responsible for storing and editing program data which can be compiled.
	/// </summary>
	public sealed class VisualProgram {

		public VisualProgram() : this(_ => { }) { }
		public VisualProgram(Action<VisualProgramEnvironmentBuilder> environmentConfig) : this(environmentConfig, new VisualNodeCollection(), new VariableCollection()) { }
		public VisualProgram(Action<VisualProgramEnvironmentBuilder> environmentConfig, VisualNodeCollection nodes, VariableCollection variables) {
			var builder = new VisualProgramEnvironmentBuilder();
			environmentConfig(builder);
			Environment = builder.Build();

			Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
			Variables = variables ?? throw new ArgumentNullException(nameof(variables));
			Variables.context = this;
			Variables.MergeWithEnvironment();
		}

		public VisualProgramEnvironment Environment { get; }

		public VisualNodeCollection Nodes { get; }

		public VariableCollection Variables { get; }

        /// <summary>
        /// Compiles the VisualProgram into a compiled program factory, which can then be used to create independent instances of the program.<para/>
		/// The program instances generated from this factory will attempt to implement the given interface.
        /// </summary>
        public CompiledProgramFactory<TImplements> Compile<TImplements>() where TImplements : class => new CompiledProgramFactory<TImplements>(
			Nodes.OfType<VisualEntry>().ToDictionary(e => e.VisualEntryId, e => e.CreateLambda(this).Compile()),
			Variables
		);

		/// <summary>
		/// Compiles the VisualProgram into a compiled program factory, which can then be used to create independent instances of the program.
		/// </summary>
		public CompiledProgramFactory<IAnonymousProgram> Compile() => Compile<IAnonymousProgram>();
    }
}

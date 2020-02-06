using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VisualProgrammer.Core.Environment {

	/// <summary>
	/// A fluent API for controlling which visual nodes are available for an environment.
	/// </summary>
	public sealed class VisualNodeConfigurator {

		private readonly HashSet<Type> includedTypes = new HashSet<Type>();
		private readonly HashSet<Type> excludedTypes = new HashSet<Type>();

		internal VisualNodeConfigurator() { }

		internal IEnumerable<Type> Types => includedTypes.Except(excludedTypes);

		/// <summary>
		/// Adds all the default node types included in the VisualProgrammer.Core assembly.
		/// </summary>
		public VisualNodeConfigurator IncludeDefault() => IncludeFromAssembly(typeof(VisualNodeConfigurator).Assembly);

		/// <summary>
		/// Adds all the node types included in the given assembly.
		/// </summary>
		/// <param name="assembly">The assembly to add the types from.</param>
		public VisualNodeConfigurator IncludeFromAssembly(Assembly assembly) => Include(assembly.GetTypes().Where(t => typeof(VisualNode).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface));

		/// <summary>
		/// Adds all node types in all assemblies loaded in the current <see cref="AppDomain"/>.
		/// </summary>
		public VisualNodeConfigurator IncludeFromAppDomain() {
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				IncludeFromAssembly(asm);
			return this;
		}

		/// <summary>
		/// Adds a single node type to the environment.
		/// </summary>
		public VisualNodeConfigurator Include(Type type) {
			if (typeof(VisualNode).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
				includedTypes.Add(type);
			return this;
		}

		/// <summary>
		/// Adds multiple node types to the environment.
		/// </summary>
		public VisualNodeConfigurator Include(IEnumerable<Type> types) {
			foreach (var type in types)
				Include(type);
			return this;
		}

		/// <summary>
		/// Adds multiple node types to the environment.
		/// </summary>
		public VisualNodeConfigurator Include(params Type[] types) => Include((IEnumerable<Type>)types);

		/// <summary>
		/// Adds the given node type to the environment.
		/// </summary>
		public VisualNodeConfigurator Include<T>() where T : VisualNode => Include(typeof(T));

		/// <summary>
		/// Excludes one or more types from the environment.<para/>
		/// Note that the exclusion is performed at the end, so if "Include" is called after this method with the same
		/// type, that type will not be re-included in the environment.
		/// </summary>
		public VisualNodeConfigurator Exclude(params Type[] types) {
			foreach (var type in types)
				excludedTypes.Add(type);
			return this;
		}

		/// <summary>
		/// Excludes the given type from the environment.<para/>
		/// Note that the exclusion is performed at the end, so if "Include" is called after this method with the same
		/// type, that type will not be re-included in the environment.
		/// </summary>
		public VisualNodeConfigurator Exclude<T>() where T : VisualNode => Exclude(typeof(T));
	}
}

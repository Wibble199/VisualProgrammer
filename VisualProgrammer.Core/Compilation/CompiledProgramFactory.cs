using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.Emit.OpCodes;

namespace VisualProgrammer.Core.Compilation {

	public class CompiledProgramFactory<TImplements> where TImplements : class {

		// Builders
		private static readonly AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("VisualProgrammerCompiledPrograms"), AssemblyBuilderAccess.Run);
		private static readonly ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

		// Cached reflection info for creating the dynamic type
		private static readonly ConstructorInfo cibCtor = typeof(CompiledInstanceBase).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];
		private static readonly MethodInfo cibInvoke = typeof(CompiledInstanceBase).GetMethod(nameof(CompiledInstanceBase.ExecuteFunction), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		private static readonly MethodInfo arrEmptyObj = typeof(Array).GetMethod("Empty").MakeGenericMethod(typeof(object));


		// Dynamic type that is generated from the functions passed to the CompiledProgram ctor.
		private readonly Dictionary<string, Delegate> functions;
		private readonly Dictionary<string, (Type, object)> varDefs;
		private readonly Type programType;


		internal CompiledProgramFactory(Dictionary<string, Delegate> functions, Dictionary<string, (Type type, object @default)> variableDefinitions) {
			// Guard to ensure the TImplements in an interface
			if (!typeof(TImplements).IsInterface)
				throw new ArgumentException($"Type parameter '{nameof(TImplements)}' must be an interface.", nameof(TImplements));

			this.functions = functions;
			varDefs = variableDefinitions;

			// Create a new type that implements the TImplements type (and also has other methods as per the function IDs)
			var typeBuilder = moduleBuilder.DefineType("Dynamic_" + Guid.NewGuid().ToString(), TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, typeof(CompiledInstanceBase));
			typeBuilder.AddInterfaceImplementation(typeof(TImplements));

			// Generate constructor
			var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.HasThis, new[] { typeof(Dictionary<string, Delegate>) });
			var ctorIl = ctorBuilder.GetILGenerator();
			ctorIl.Emit(Ldarg_0);
			ctorIl.Emit(Ldarg_1);
			ctorIl.Emit(Call, cibCtor);
			ctorIl.Emit(Ret);

			// For each defined method on the interface, generate the IL to invoke it.
			foreach (var method in typeof(TImplements).GetMethods().Where(m => m.ReturnType == typeof(void))) {
				var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual);
				GenerateDynamicTypeMethod(methodBuilder.GetILGenerator(), method.Name, method.GetParameters().Length);
				typeBuilder.DefineMethodOverride(methodBuilder, method); // Indicate this method overrides the one on the interface
			}

			programType = typeBuilder.CreateType();
		}

		/// <summary>
		/// Outputs the IL on the given <see cref="ILGenerator"/> that forwards the parameters to <see cref="CompiledInstanceBase.ExecuteFunction(string, object[])"/>.
		/// </summary>
		/// <param name="il">The generator that will get the emitted IL.</param>
		/// <param name="methodName">The method name string that will be passed to <see cref="CompiledInstanceBase.ExecuteFunction(string, object[])"/>.</param>
		/// <param name="numParams">The number of parameters to be passed to <see cref="CompiledInstanceBase.ExecuteFunction(string, object[])"/>.</param>
		private void GenerateDynamicTypeMethod(ILGenerator il, string methodName, int numParams) {
			il.Emit(Ldarg_0);
			il.Emit(Ldstr, methodName);
			if (numParams == 0)
				// If no arguments, get an empty array (using Array.Empty<object>() ) to pass to "CompiledInstanceBase.ExecuteFunction"
				il.Emit(Call, arrEmptyObj);
			else {
				// Create an object array with numParams elements
				il.Emit(Ldc_I4, numParams);
				il.Emit(Newarr, typeof(object));

				// Load each parameter from the method into the array
				for (var i = 0; i < numParams; i++) {
					il.Emit(Dup);
					il.Emit(Ldc_I4, i);
					il.Emit(Ldarg, i + 1);
					il.Emit(Box);
					il.Emit(Stelem_Ref);
				}
			}
			il.Emit(Call, cibInvoke);
			il.Emit(Pop);
			il.Emit(Ret);
		}


		/// <summary>
		/// Creates a new instance of the target program, with its own set of variables.<para/>
		/// Note that the returned type also inherits <see cref="CompiledInstanceBase"/> (and, by extension, <see cref="System.Dynamic.DynamicObject"/>).
		/// </summary>
		public TImplements CreateProgram() => (TImplements)Activator.CreateInstance(programType, functions);
	}
}

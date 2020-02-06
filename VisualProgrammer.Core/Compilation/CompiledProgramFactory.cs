using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.Emit.OpCodes;

namespace VisualProgrammer.Core.Compilation {

	/// <summary>
	/// A factory class that is produced as the result of the compilation of a <see cref="VisualProgram"/>.
	/// This can be used to create self-containted instances of the program.<para/>
	/// A type parameter can be provided which should be an interface which will be implemented by the resulting instance of the program.
	/// Any methods defined on the interface will map to a function with the same name as it's key. Any properties on the program will
	/// map to variables of the same name. Note that any variables not mapped to an interface's property will only available for reading/writing
	/// via the <see cref="CompiledInstanceBase.Variables"/> property, however all functions in the VisualProgram that are available on the root program instance.
	/// </summary>
	/// <typeparam name="TImplements">An interface type that is implemented by the program instances created by this factory.</typeparam>
	public class CompiledProgramFactory<TImplements> where TImplements : class {

		// Builders
		private static readonly AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("VisualProgrammerCompiledPrograms"), AssemblyBuilderAccess.Run);
		private static readonly ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

		// Cached reflection info for creating the dynamic type
		private static readonly ConstructorInfo cibCtor = typeof(CompiledInstanceBase).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)[0];
		private static readonly MethodInfo cibInvoke = typeof(CompiledInstanceBase).GetMethod(nameof(CompiledInstanceBase.ExecuteFunction), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		private static readonly MethodInfo arrEmptyObj = typeof(Array).GetMethod("Empty").MakeGenericMethod(typeof(object));
		private static readonly MethodInfo getVariable = typeof(CompiledInstanceBase).GetMethod(nameof(CompiledInstanceBase.GetVariable), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		private static readonly MethodInfo setVariable = typeof(CompiledInstanceBase).GetMethod(nameof(CompiledInstanceBase.SetVariable), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


		// Dynamic type that is generated from the functions passed to the CompiledProgram ctor.
		private readonly Dictionary<string, Delegate> functions;
		private readonly Dictionary<string, Variable> vars;
		private readonly Type programType;


		internal CompiledProgramFactory(Dictionary<string, Delegate> functions, IEnumerable<Variable> variableDefinitions) {
			// Guard to ensure the TImplements in an interface
			if (!typeof(TImplements).IsInterface)
				throw new ArgumentException($"Type parameter '{nameof(TImplements)}' must be an interface.", nameof(TImplements));

			this.functions = functions;
            vars = variableDefinitions.ToDictionary(v => v.Name, v => v);

			// Create a new type that implements the TImplements type (and also has other methods as per the function IDs)
			var typeBuilder = moduleBuilder.DefineType("Dynamic_" + Guid.NewGuid().ToString(), TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, typeof(CompiledInstanceBase));
			typeBuilder.AddInterfaceImplementation(typeof(TImplements));

			// Generate constructor
			var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.HasThis, new[] { typeof(Dictionary<string, Delegate>), typeof(Dictionary<string, Variable>) });
			var ctorIl = ctorBuilder.GetILGenerator();
			ctorIl.Emit(Ldarg_0);
			ctorIl.Emit(Ldarg_1);
			ctorIl.Emit(Ldarg_2);
			ctorIl.Emit(Call, cibCtor);
			ctorIl.Emit(Ret);

			// For each defined method on the interface, generate the IL to invoke it.
			foreach (var method in typeof(TImplements).GetMethods().Where(m => m.ReturnType == typeof(void) && !m.IsSpecialName)) {
				var @params = method.GetParameters().Select(p => p.ParameterType).ToArray();
				var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(void), @params);
				GenerateDynamicTypeMethod(methodBuilder.GetILGenerator(), method.Name, @params);
				typeBuilder.DefineMethodOverride(methodBuilder, method); // Indicate this method overrides the one on the interface
			}

			// For each defined property on the interface, generate the IL to access it.
			foreach (var prop in typeof(TImplements).GetProperties())
				GenerateDynamicTypeProperty(typeBuilder, prop);

			programType = typeBuilder.CreateType();
		}

		/// <summary>
		/// Outputs the IL on the given <see cref="ILGenerator"/> that forwards the parameters to <see cref="CompiledInstanceBase.ExecuteFunction(string, object[])"/>.
		/// </summary>
		/// <param name="il">The generator that will get the emitted IL.</param>
		/// <param name="methodName">The method name string that will be passed to <see cref="CompiledInstanceBase.ExecuteFunction(string, object[])"/>.</param>
		/// <param name="params">The parameters to be passed to <see cref="CompiledInstanceBase.ExecuteFunction(string, object[])"/>.</param>
		private void GenerateDynamicTypeMethod(ILGenerator il, string methodName, Type[] @params) {
			il.Emit(Ldarg_0);
			il.Emit(Ldstr, methodName);
			if (@params.Length == 0)
				// If no arguments, get an empty array (using Array.Empty<object>() ) to pass to "CompiledInstanceBase.ExecuteFunction"
				il.Emit(Call, arrEmptyObj);
			else {
				// Create an object array with numParams elements
				il.Emit(Ldc_I4, @params.Length);
				il.Emit(Newarr, typeof(object));

				// Load each parameter from the method into the array
				for (var i = 0; i < @params.Length; i++) {
					il.Emit(Dup);
					il.Emit(Ldc_I4, i);
					il.Emit(Ldarg, i + 1);
					il.Emit(Box, @params[i]);
					il.Emit(Stelem_Ref);
				}
			}
			il.Emit(Call, cibInvoke);
			il.Emit(Pop);
			il.Emit(Ret);
		}

		/// <summary>
		/// Creates a property on the target type builder that implements the given property info (from an interface).
		/// </summary>
		/// <param name="typeBuilder">The type builder that the property will be added to.</param>
		/// <param name="prop">The property definition from the base interface that will be implemented.</param>
		private void GenerateDynamicTypeProperty(TypeBuilder typeBuilder, PropertyInfo prop) {
			var propBuilder = typeBuilder.DefineProperty(prop.Name, PropertyAttributes.None, prop.PropertyType, Type.EmptyTypes);
			if (prop.CanRead) {
				var getBuilder = typeBuilder.DefineMethod($"get_{prop.Name}", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName, prop.PropertyType, Type.EmptyTypes);
				var il = getBuilder.GetILGenerator();
				il.Emit(Ldarg_0);
				il.Emit(Ldstr, prop.Name);
				il.Emit(Call, getVariable);
				il.Emit(Unbox_Any, prop.PropertyType);
				il.Emit(Ret);
				typeBuilder.DefineMethodOverride(getBuilder, prop.GetMethod);
				propBuilder.SetGetMethod(getBuilder);
			}
			if (prop.CanWrite) {
				var setBuilder = typeBuilder.DefineMethod($"set_{prop.Name}", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName, typeof(void), new[] { prop.PropertyType });
				var il = setBuilder.GetILGenerator();
				il.Emit(Ldarg_0);
				il.Emit(Ldstr, prop.Name);
				il.Emit(Ldarg_1);
				il.Emit(Box, prop.PropertyType);
				il.Emit(Call, setVariable);
				il.Emit(Ret);
				typeBuilder.DefineMethodOverride(setBuilder, prop.SetMethod);
				propBuilder.SetSetMethod(setBuilder);
			}
		}


		/// <summary>
		/// Creates a new instance of the target program, with its own set of variables.<para/>
		/// Note that the returned type also inherits <see cref="CompiledInstanceBase"/> (and, by extension, <see cref="System.Dynamic.DynamicObject"/>).
		/// </summary>
		public TImplements CreateProgram() => (TImplements)Activator.CreateInstance(programType, functions, vars);
	}
}

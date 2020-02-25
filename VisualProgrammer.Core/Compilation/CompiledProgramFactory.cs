using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using VisualProgrammer.Core.Utils;
using static System.Reflection.Emit.OpCodes;
using VariableStore = System.Collections.Generic.Dictionary<string, VisualProgrammer.Core.Variable>;
using FunctionStore = System.Collections.Generic.Dictionary<string, System.Delegate?>;

namespace VisualProgrammer.Core.Compilation {

	/// <summary>
	/// A factory class that is produced as the result of the compilation of a <see cref="VisualProgram"/>.
	/// This can be used to create self-containted instances of the program.<para/>
	/// A type parameter can be provided which can be a class or an interface which will be extended/implemented by the resulting instance of the program.
	/// If a class is given, any abstract or virtual methods on the class will be overriden with calls to the relevant entry functions. Note that if an
	/// entry is empty (i.e not added to the program canvas or does not have any attached statements), virtual methods will NOT be overriden, only the
	/// relevant abstract methods. This allows for providing a default implementation if the user does not specify any behaviour. Any virtual or abstract 
	/// properties on the class will be mapped to a matching variable. If the base class has any constructors defined, any parameters passed to
	/// <see cref="CreateProgram"/> will be passed to the relevant base constructor.<para/>
	/// If an interface is given, any methods defined on that interface will map to an entry with the same name. Any properties on the inter will
	/// map to variables of the same name.<para/>
	/// Note that any variables not mapped to an interface or class's properties will only available for reading/writing via the
	/// <see cref="ICompiledInstanceBase.GetVariable(string)"/> and <see cref="ICompiledInstanceBase.SetVariable(string, object)"/> methods. (to prevent
	/// name collisions between variables and entries), however all functions in the VisualProgram that are available on the root program instance.
	/// </summary>
	/// <typeparam name="TExtends">A class or interface type that is extended/implemented by the program instances created by this factory.</typeparam>
	public class CompiledProgramFactory<TExtends> where TExtends : class {

		// Builders
		private static readonly AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("VisualProgrammerCompiledPrograms"), AssemblyBuilderAccess.Run);
		private static readonly ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

		// Names of some fields
		private const string FunctionsFieldName = "_functions";
		private const string VariablesFieldName = "_variables";

		// Cached reflection info
		private static readonly MethodInfo variableDictGetItem = typeof(VariableStore).GetMethod("get_Item");
		private static readonly MethodInfo functionDictGetItem = typeof(FunctionStore).GetMethod("get_Item");
		private static readonly MethodInfo variableGetValue = typeof(Variable).GetMethod("get_Value");
		private static readonly MethodInfo variableSetValue = typeof(Variable).GetMethod("set_Value");

		// Some static method signatures
		private static readonly Type[] getVariableSignature = new[] { typeof(string) };
		private static readonly Type[] setVariableSignature = new[] { typeof(string), typeof(object) };

		// Builders used when creating the dynamic type
		private readonly TypeBuilder typeBuilder;
		private readonly FieldBuilder functionsFieldBuilder;
		private readonly FieldBuilder varsFieldBuilder;
		private MethodBuilder getVariableMethodBuilder;
		private MethodBuilder setVariableMethodBuilder;
		private MethodBuilder resetVariablesMethodBuilder;

		private readonly FieldInfo varsFieldInfo;

		private readonly VisualProgram program;
		private readonly FunctionStore functions;
		private readonly VariableStore vars;

		// Dynamic type that is generated from the functions passed to the CompiledProgram ctor.
		private readonly Type programType;


		[SuppressMessage("", "CS8618", Justification = "These non-nullable fields are always set during the 'GenerateDefaultMethods' call.")]
		internal CompiledProgramFactory(VisualProgram program) {
			this.program = program;
			functions = CompileEntryDelegates(program);
            vars = program.Variables.ToDictionary(v => v.Name, v => v.Clone(), StringComparer.OrdinalIgnoreCase);

			// Create a new type that extends/implements the TExtends type
			typeBuilder = moduleBuilder.DefineType("Dynamic_" + Guid.NewGuid().ToString("N"), TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout);
			if (typeof(TExtends).IsInterface)
				typeBuilder.AddInterfaceImplementation(typeof(TExtends));
			else
				typeBuilder.SetParent(typeof(TExtends));

			// Add the ICompiledInstanceBase (so long as the TExtends wasn't that)
			if (typeof(TExtends) != typeof(ICompiledInstanceBase))
				typeBuilder.AddInterfaceImplementation(typeof(ICompiledInstanceBase));

			// Generate the fields that will hold our compiled functions and variables
			functionsFieldBuilder = typeBuilder.DefineField(FunctionsFieldName, typeof(FunctionStore), FieldAttributes.Private | FieldAttributes.Static);
			varsFieldBuilder = typeBuilder.DefineField(VariablesFieldName, typeof(VariableStore), FieldAttributes.Private);

			GenerateConstructors();
			GenerateDefaultMethods();
			GenerateDelegateBindings();
			GeneratePropertyBindings();

			// Create the type
			programType = typeBuilder.CreateType();

			// Set the value of the static functions field
			programType.GetField(FunctionsFieldName, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, functions);
			varsFieldInfo = programType.GetField(VariablesFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		}

		/// <summary>
		/// Compiles the entries in the given Program into delegates.
		/// Returns the delegates themselves and the delegate signatures (excluding the context parameter).
		/// </summary>
		private FunctionStore CompileEntryDelegates(VisualProgram program) {
			var progEntryNodes = program.Nodes.OfType<VisualEntry>().ToDictionary(ve => ve.VisualEntryId, ve => ve);
			return program.Environment.EntryDefinitions.ToDictionary(
				entry => entry.Key,
				entry => {
					// If the program does not contain this entry node OR it exists but is not connected to any statements, then return no delegate
					if (!progEntryNodes.TryGetValue(entry.Value.Id, out var startNode) || !startNode.FirstStatement.HasValue)
						return null;

					// Create a parameter for each expected/defined parameter
					var parameters = entry.Value.Parameters.Map(Expression.Parameter);

					// Compile and return the lambda
					return Expression.Lambda(
						Expression.Block(
							// Before doing the main body of the entry function, make expressions to copy the incoming parameters to variables
							startNode.ParameterMap
								.Where(mapping => !string.IsNullOrWhiteSpace(mapping.Value)) // Exclude any that don't actually map to anything
								.Select(p => VariableAccessorFactory.CreateSetterExpression(
									program,
									VariableReference.Create(entry.Value.Parameters[p.Key], p.Value),
									parameters[p.Key]
								)
							).Concat(
								// Then do the actual main body
								NodeUtils.FlattenExpressions(program, startNode.FirstStatement)
							)
						),

						// All lambdas will get a context parameter
						// We also pass the defined parameters to the lambda so it knows what types to expect and what signature to have.
						// This needs to happen regardless or not of whether the parameters are actually used by the entry function as this determines the delegate's signature.
						new[] { program.Variables.compiledInstanceParameter }.Concat(parameters)
					).Compile();
				},
				StringComparer.OrdinalIgnoreCase
			);
		}

		/// <summary>
		/// Creates a constructor on the type for matching each constructor on the base type (<typeparamref name="TExtends"/>) or, if the base type is an interface,
		/// creates a default parameterless constructor.
		/// </summary>
		private void GenerateConstructors() {
			// Constructor bindingflags
			const MethodAttributes bf = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

			if (typeof(TExtends).IsInterface) {
				// For interfaces, we need make a default parameterless one
				typeBuilder.DefineDefaultConstructor(bf);

			} else {
				// Otherwise, make a constructor for each one on the base class with matching args
				foreach (var ctor in typeof(TExtends).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
					var @params = ctor.GetParameters();
					var ctorBuilder = typeBuilder.DefineConstructor(bf, CallingConventions.HasThis, @params.Select(p => p.ParameterType).ToArray());
					var il = ctorBuilder.GetILGenerator();
					il.Emit(Ldarg_0);
					for (var i = 0; i < @params.Length; i++)
						il.Emit(Ldarg, i + 1);
					il.Emit(Call, ctor);
					il.Emit(Ret);
				}
			}
		}

		/// <summary>
		/// Creates default methods that are part of the <see cref="ICompiledInstanceBase"/> interface.
		/// </summary>
		private void GenerateDefaultMethods() {
			// object GetVariable(string)
			getVariableMethodBuilder = typeBuilder.DefineMethod(nameof(ICompiledInstanceBase.GetVariable), MethodAttributes.Public | MethodAttributes.Virtual, typeof(object), getVariableSignature);
			var il = getVariableMethodBuilder.GetILGenerator();
			CheckVarExists(il);
			il.Emit(Ldarg_0);
			il.Emit(Ldfld, varsFieldBuilder);
			il.Emit(Ldarg_1);
			il.Emit(Callvirt, variableDictGetItem);
			il.Emit(Callvirt, typeof(Variable).GetMethod("get_Value"));
			il.Emit(Ret);
			typeBuilder.DefineMethodOverride(getVariableMethodBuilder, typeof(ICompiledInstanceBase).GetMethod(nameof(ICompiledInstanceBase.GetVariable)));
			TryDefineMethodOverride(nameof(ICompiledInstanceBase.GetVariable), typeof(object), getVariableSignature, getVariableMethodBuilder);

			// void SetVariable(string, object)
			setVariableMethodBuilder = typeBuilder.DefineMethod(nameof(ICompiledInstanceBase.SetVariable), MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), setVariableSignature);
			il = setVariableMethodBuilder.GetILGenerator();
			CheckVarExists(il);
			il.Emit(Ldarg_0);
			il.Emit(Ldfld, varsFieldBuilder);
			il.Emit(Ldarg_1);
			il.Emit(Callvirt, variableDictGetItem);
			il.Emit(Ldarg_2);
			il.Emit(Callvirt, typeof(Variable).GetMethod("set_Value"));
			il.Emit(Ret);
			typeBuilder.DefineMethodOverride(setVariableMethodBuilder, typeof(ICompiledInstanceBase).GetMethod(nameof(ICompiledInstanceBase.SetVariable)));
			TryDefineMethodOverride(nameof(ICompiledInstanceBase.SetVariable), typeof(void), setVariableSignature, setVariableMethodBuilder);

			// void ResetVariables()
			resetVariablesMethodBuilder = typeBuilder.DefineMethod(nameof(ICompiledInstanceBase.ResetVariables), MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), Type.EmptyTypes);
			il = resetVariablesMethodBuilder.GetILGenerator();
			var enumeratorLocal = il.DeclareLocal(typeof(VariableStore.Enumerator));
			var kvpLocal = il.DeclareLocal(typeof(KeyValuePair<string, Variable>));
			var moveNextLabel = il.DefineLabel();
			var loopStartLabel = il.DefineLabel();
			var returnLabel = il.DefineLabel();

			il.Emit(Ldarg_0);
			il.Emit(Ldfld, varsFieldBuilder);
			il.Emit(Callvirt, typeof(VariableStore).GetMethod(nameof(VariableStore.GetEnumerator)));
			il.Emit(Stloc_0);

			il.BeginExceptionBlock();
			il.Emit(Br_S, moveNextLabel);
			il.MarkLabel(loopStartLabel);
			il.Emit(Ldloca_S, enumeratorLocal.LocalIndex);
			il.Emit(Call, typeof(VariableStore.Enumerator).GetMethod("get_Current"));
			il.Emit(Stloc_1);
			il.Emit(Ldloca_S, kvpLocal.LocalIndex);
			il.Emit(Call, typeof(KeyValuePair<string, Variable>).GetMethod("get_Value"));
			il.Emit(Callvirt, typeof(Variable).GetMethod(nameof(Variable.Reset)));

			il.MarkLabel(moveNextLabel);
			il.Emit(Ldloca_S, enumeratorLocal.LocalIndex);
			il.Emit(Call, typeof(VariableStore.Enumerator).GetMethod(nameof(VariableStore.Enumerator.MoveNext)));
			il.Emit(Brtrue_S, loopStartLabel);

			il.Emit(Leave_S, returnLabel);

			il.BeginFinallyBlock();
			il.Emit(Ldloca_S, enumeratorLocal.LocalIndex);
			il.Emit(Constrained, enumeratorLocal.LocalType);
			il.Emit(Callvirt, typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)));
			il.Emit(Endfinally);
			il.EndExceptionBlock();

			il.MarkLabel(returnLabel);
			il.Emit(Ret);

			typeBuilder.DefineMethodOverride(resetVariablesMethodBuilder, typeof(ICompiledInstanceBase).GetMethod(nameof(ICompiledInstanceBase.ResetVariables)));
		}

		/// <summary>
		/// Adds variable existance check code to the given IL generator. This is code that is shared between getVar and setVar.
		/// </summary>
		/// <param name="il">The IL generator to add the IL to.</param>
		private void CheckVarExists(ILGenerator il) {
			var validVarLabel = il.DefineLabel();

			il.Emit(Ldarg_0);
			il.Emit(Ldfld, varsFieldBuilder);
			il.Emit(Ldarg_1);
			il.Emit(Callvirt, typeof(VariableStore).GetMethod(nameof(VariableStore.ContainsKey)));
			il.Emit(Brtrue_S, validVarLabel);

			il.Emit(Ldstr, "Variable '");
			il.Emit(Ldarg_1);
			il.Emit(Ldstr, "' has not been defined");
			il.Emit(Call, typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string), typeof(string) }));
			il.Emit(Ldstr, "key");
			il.Emit(Newobj, typeof(ArgumentException).GetConstructor(new[] { typeof(string), typeof(string) }));
			il.Emit(Throw);

			il.MarkLabel(validVarLabel);
		}

		/// <summary>
		/// Implements methods for each entry defined in the program.
		/// </summary>
		private void GenerateDelegateBindings() {
			foreach (var func in functions) {
				var args = program.Environment.EntryDefinitions[func.Key].Parameters.Values.ToArray();
				var isEmpty = func.Value == null;
				var declaration = typeof(TExtends).GetMethod(func.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy, null, args, null);

				// If the method declaration on TExtends is a non-abstract virtual method (i.e. it already has an implementation) AND the incoming compiled function is empty, don't override the default implementation
				if (declaration?.IsAbstract == false && isEmpty) continue;

				// If a declarion is found, use the casing from that declaration.
				var name = declaration?.Name ?? func.Key;

				// The visibility of the new method is the same as the existing declaration if it exists, or defaults to public if it does not.
				var newVisibility = declaration == null ? MethodAttributes.Public : IsolateVisibilityAttribute(declaration.Attributes);

				// If a declaration exists, make sure we use the casing from that declarion
				var method = typeBuilder.DefineMethod(name, newVisibility | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(void), args);
				var il = method.GetILGenerator();

				if (!isEmpty) {
					// Note that we cast to an Action of the relevant type then call "Invoke" on that because Action.Invoke is DRAMATICALLY quicker than using Delegate.DynamicInvoke (https://stackoverflow.com/a/12858434)
					// It also simplifies the generated IL code since we just need to 'Ldarg' each argument onto the stack instead of creating and assigning an object array (as you would for DynamicInvoke)
					var actionType = Expression.GetActionType(new[] { typeof(ICompiledInstanceBase) }.Concat(args).ToArray());
					il.Emit(Ldsfld, functionsFieldBuilder);
					il.Emit(Ldstr, func.Key);
					il.Emit(Callvirt, functionDictGetItem);
					il.Emit(Castclass, actionType);
					for (var i = 0; i <= args.Length; i++)
						il.Emit(Ldarg, i);
					il.Emit(Callvirt, actionType.GetMethod(nameof(Action.Invoke)));
				}

				// If the compiled lambda function is empty, simply make an empty method that immediately returns (instead of going through the step of invoking an empty method)
				il.Emit(Ret);

				TryDefineMethodOverride(name, typeof(void), args, method, false);
			}
		}

		/// <summary>
		/// Implements or overrides any abstract or virtual properties defined on <typeparamref name="TExtends"/> with accessors to the variable of the same name.
		/// </summary>
		private void GeneratePropertyBindings() {
			foreach (var prop in typeof(TExtends).GetProperties()) {
				// Check if a variable of that name exists and the types match
				if (vars.TryGetValue(prop.Name, out var varDef) && varDef.Type == prop.PropertyType) {
					// Determine if the property on the class/interface can be overriden (i.e. a setter or getter is abstract/virtual)
					var hasOverridableGetter = prop.GetMethod != null && (prop.GetMethod.IsVirtual || prop.GetMethod.IsAbstract);
					var hasOverridableSetter = prop.SetMethod != null && (prop.SetMethod.IsVirtual || prop.SetMethod.IsAbstract);

					if (!(hasOverridableGetter || hasOverridableSetter)) continue;

					// If atleast the setter or getter is overridable, implement the property
					var propBuilder = typeBuilder.DefineProperty(prop.Name, prop.Attributes, prop.PropertyType, null);

					// If the getter is overridable, implement the getter (which is a proxy for the 'Value' property of the variable with the relevant key in the variable dictionary)
					if (hasOverridableGetter) {
						var vis = IsolateVisibilityAttribute(prop.GetMethod!.Attributes);
						var propGetBuilder = typeBuilder.DefineMethod($"get_{prop.Name}", vis | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, prop.PropertyType, Type.EmptyTypes);
						var il = propGetBuilder.GetILGenerator();
						il.Emit(Ldarg_0);
						il.Emit(Ldfld, varsFieldBuilder);
						il.Emit(Ldstr, varDef.Name);
						il.Emit(Callvirt, variableDictGetItem);
						il.Emit(Callvirt, variableGetValue);
						il.Emit(Unbox_Any, prop.PropertyType);
						il.Emit(Ret);
						propBuilder.SetGetMethod(propGetBuilder);
						TryDefineMethodOverride($"get_{prop.Name}", prop.PropertyType, Type.EmptyTypes, propGetBuilder);
					}

					// If the setter is overridable, implement it (which is a proxy for setting the 'Value' property of the variable with the relevant key in the variable dictionary)
					if (hasOverridableSetter) {
						var vis = IsolateVisibilityAttribute(prop.SetMethod!.Attributes);
						var propSetBuilder = typeBuilder.DefineMethod($"set_{prop.Name}", vis | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeof(void), new[] { prop.PropertyType });
						var il = propSetBuilder.GetILGenerator();
						il.Emit(Ldarg_0);
						il.Emit(Ldfld, varsFieldBuilder);
						il.Emit(Ldstr, varDef.Name);
						il.Emit(Callvirt, variableDictGetItem);
						il.Emit(Ldarg_1);
						il.Emit(Box, prop.PropertyType);
						il.Emit(Callvirt, variableSetValue);
						il.Emit(Ret);
						propBuilder.SetSetMethod(propSetBuilder);
						TryDefineMethodOverride($"set_{prop.Name}", typeof(void), new[] { prop.PropertyType }, propSetBuilder);
					}
				}
			}
		}

		/// <summary>
		/// Attempts to mark the given method as an override for a method on <typeparamref name="TExtends"/> (and it's superclasses) with the given <paramref name="name"/> and <paramref name="parameterTypes"/>.<para/>
		/// </summary>
		/// <param name="overrideVirtual">If false and and the found method is virtual but not abstract (n.b. this includes default interface methods), the override will not be set.
		/// This is for when overriding a method that calls a function and the compiled function is empty.</param>
		private void TryDefineMethodOverride(string name, Type returnType, Type[] parameterTypes, MethodBuilder methodBuilder, bool overrideVirtual = true) {
			// Attempts to find and override a method on the given type
			void DefineOnType(Type targetType) {
				if (targetType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy, null, parameterTypes, null) is { } targetDeclaration && targetDeclaration.ReturnType == returnType && (targetDeclaration.IsAbstract || (overrideVirtual && targetDeclaration.IsVirtual)))
					typeBuilder.DefineMethodOverride(methodBuilder, targetDeclaration);
			}

			// Override methods on the starting TExtends type (flattening the heirarchy, so any abstract/virtual methods in superclasses will also be overriden)
			DefineOnType(typeof(TExtends));

			// Attempt to mark an override in any interfaces implemented by TExends.
			foreach (var @interface in typeof(TExtends).GetInterfaces())
				DefineOnType(@interface);
		}

		/// <summary>
		/// Isolates the visibility modifier from the given <see cref="MethodAttributes"/>.
		/// </summary>
		private static MethodAttributes IsolateVisibilityAttribute(MethodAttributes existing) => existing & MethodAttributes.MemberAccessMask;

		/// <summary>
		/// Creates a new instance of the target program, with its own set of variables.<para/>
		/// Note that the returned type also inherits <see cref="ICompiledInstanceBase"/> (and, by extension, <see cref="System.Dynamic.DynamicObject"/>).
		/// </summary>
		/// <exception cref="ArgumentException">If no constructor could be found with the given parameters.</exception>
		public TExtends CreateProgram(params object[] args) {
			TExtends inst;
			try {
				inst = (TExtends)Activator.CreateInstance(programType, BindingFlags.Instance | BindingFlags.NonPublic, null, args, null);
			} catch (MissingMethodException ex) {
				throw new ArgumentException("Could not find a suitable constructor for the given parameters.", nameof(args), ex);
			}
			varsFieldInfo.SetValue(inst, vars.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase));
			((ICompiledInstanceBase)inst).ResetVariables();
			return inst;
		}
	}
}

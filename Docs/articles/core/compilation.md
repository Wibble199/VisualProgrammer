# Compilation

When a VisualProgram is compiled using the Compile methods, a program factory class is created. During compilation, a new dynamic class is created which extends [`CompiledInstanceBase`](xref:VisualProgrammer.Core.Compilation.CompiledInstanceBase) and optionally implements a target interface (or a blank `IAnonymousProgram` if no interface is specified). This generated factory can then be used to create self-contained instances of the compiled program. These instances store their variables and compiled functions and provide methods to access them.

The `CompiledInstanceBase` extends `System.Dynamic.DynamicObject` and exposes all compiled entries as invokable members of the class. Any variables can be accessed using `GetVariable(string)` or `SetVariable(string, object)`.

If an interface was provided to the compile method of the `VisualProgram`, the factory attempts to bind any methods defined on the interface to a compiled entry function of the same name. It also attempts to bind any properties defined on the interface to a variable of the same name and type. Note that any method on the interface should have the same signature as the entry it is assigned to.

## Example
```cs
// Our program definition
VisualProgram program = new VisualProgram {
	EntryDefinitions = new Dictionary<string, EntryDefinition> {
		["paramEntry"] = new EntryDefinition {
			Name = "Parameterized Entry",
			Parameters = new IndexedDictionary<string, Type> {
				{ "someDouble", typeof(double) }
			}
		},
		["nonParamEntry"] = new EntryDefinition {
			Name = "No paremeters"
		}
	},

	variableDefinitions = new Dictionary<string, (Type type, object @default)> {
		["doubleVar"] = (typeof(double), 0d),
		["boolVar"] = (typeof(bool), false)
	},

	Nodes = new Dictionary<Guid, VisualNode> {
		...
	}
};


// Compile into an anonymous program
var anonFactory = program.Compile();
var anonInstance = anonFactory.CreateProgram();

// To invoke methods on an anonymous we must cast it to dynamic first
((dynamic)anonInstance).ParamEntry(3.14d);
((dynamic)anonInstance).NonParamEntry();
// To get/set the variables, we must first cast to a CompiledInstanceBase
Console.WriteLine("Bool var = " + ((CompiledInstanceBase)typedInstance).GetVariable("boolVar"));


// Compile into a typed program
var typedFactory = program.Compile<IMyProgram>();
var typedInstance = typedFactory.CreateProgram();

// To invoke methods on a typed program, we can do it as normal
typedInstance.ParamEntry(10.5d);
typedInstance.NonParamEntry();
Console.WriteLine("Double var = " + typedInstance.DoubleVar); // doubleVar is on the interface, so we can access it like a normal property
Console.WriteLine("Bool var = " + typedInstance.GetVariable("boolVar")); // boolVar was not on the interface, but we can access it using GetVariable

// Our interface for the typed program
// It does not have to extend from ICompiledInstanceBase, but if it does it allows us to use ResetVariables, GetVariable and SetVariable without needing to cast to a dynamic first.
interface IMyProgram : ICompiledInstanceBase {
	void ParamEntry(double d);
	void NonParamEntry();
	double DoubleVar { get; set; }
}
```

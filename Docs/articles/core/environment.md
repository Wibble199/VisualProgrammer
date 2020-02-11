# Environment

The environment determines some of the aspects of a program:
- The available types of statements and expressions
- The data types that can be utilised
	- Types of variables that can be created by the user
	- Types available for generic nodes
- The [entry definitions](entry-definition.md)
- The locked/mandatory [variables](variables.md)


## Configuring the Environment

The environment can be configured by using the fluent API exposed by the `VisualProgramEnvironmentBuilder`. An example of this is shown below.

```cs
var program = new VisualProgram(env => env
	.ConfigureNodes(n => n
		.IncludeDefault() // Include the default nodes
		.Include<MyCustomType>() // Include some custom statement/expression
	)
	.ConfigureDataTypes(t => t
		.AddDefault() // Include the default types (bool, int, double and string)
		.Remove<int>() // Remove one of the defaults
		.Add<DateTime>() // Include another type (make sure to add nodes that support this type)
	)
	.ConfigureEntries(e => e
		.Add("entryKey", "Display Name")
		.Add("anotherEntryKey", "Entry with Params", p => p
			.WithParameter<double>("doubleParam")
		)
	)
	.ConfigureLockedVariables(v => v
		.Add<double>("MyDoubleVariable", 10d)
	)
);

```

### ConfigureNodes

Methods available on the VisualNodeConfigurator:

- `IncludeDefault()` - Includes the nodes that come built-in with VisualProgrammer.Core (recommended)
- `Include<T>()`/`Include(Type)` - Includes a single specific type.
- `IncludeFromAssembly(Assembly)` - Includes all non-abstract types that extend from `VisualStatement` or `VisualExpression` in the given assembly.
- `IncludeFromAppDomain()` - Includes all non-abstract types that extend from `VisualStatement` or `VisualExpression` in **any** of the assemblies loaded into the current AppDomain
- `Exclude<T>()`/`Exclude(Type)` - Excludes a single specific type. Note that the exclusion logic is always executed last, so even if you include this type manually after this point, the type will not appear.


### ConfigureDataTypes

Methods available on the DataTypesConfigurator:

- `AddDefault()` - Includes the standard types supported by VisualProgrammer (Boolean, Int32, Double and String)
- `Add<T>()`/`Add(Type)` - Adds a specific type.
- `Remove<T>()`/`Remove(Type)` - Removes a specific type.


### ConfigureEntries

Methods available on the VisualEntryConfigurator:

- `Add(String, String)` - Adds a parameterless entry with the given internal ID and display name.
- `Add(String, String, Action<VisualEntryParameterConfigurator>)` - Adds a new entry with the given internal ID, display name and parameters.

Methods available on the VisualEntryParameterConfigurator:

- `WithParameter<T>(String)`/`WithParameter(String, Type)` - Adds a new parameter to the entry with the given type and name. The name must be unique. Note that when multiple parameters are added, the parameters are ordered in the same order they are added using this method. For example, `.WithParameter<double>("a").WithParameter<string>("b")` would result in an entry that compiles to an `Action<double, string>`.

### ConfigureLockedVariables

Methods available on the LockedVariableConfigurator:

- `Add(String, Type, Object?)`/`Add<T>(String, T)` - Adds a new locked variable of the given name and type with the given default/starting value.
- `Add(String, Type)`/`Add<T>(String)` - Adds a new locked variable of the given name and type with an automatic default value (for example 0 for int).
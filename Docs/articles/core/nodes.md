# Nodes

Nodes are the "blocks" that users can edit in the program. Nodes can be linked together to specify the functionality of the program branch.

There are a few different types of nodes:

## Entry
Entry nodes are the main starting point of the program. These are [defined](entry-definition.md) in the VisualProgram before use. The user cannot create custom entry nodes, though they can add a single instance of any pre-defined entry.

Each entry has a single "next" statement link which can be joined to any other statement node.

The entries can also take parameters. If these parameters are to be used in the visual code, they must then be mapped onto a [program variable](variables.md) first. If the parameter is not mapped, it has no effect on the program.

When the VisualProgram is compiled, each entry is compiled into a `System.Action` using Linq expressions. See [compilation](compilation.md) for more details.

## Statement
Statements are the main actions that make up a program. Statements have an input statement link and a variable number of output links, depending on the statement. Most statements just have a single output "next" statement link, but some, such as the loop or the if statement will have multiple.

Statements may also have a number of parameters. Parameters can either be raw values or links to expression nodes.

## Expression
An expression is a node which provides a value to another expression node or to a statement node. A single expression node may provide a value to multiple other nodes, though note that when compiled, the generated IL code is embedded in the action, meaning that even if an expression _is_ used in multiple places, it's value will be evaluated each time - it does **not** evaluate once and provide a cached value.

Expressions may have a number of parameters, which can either be values or may link to other expression nodes.
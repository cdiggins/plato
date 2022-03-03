# Plato

Plato is a programming languages strict subset of C# that enforces pure functional programming practices.

Plato is designed to work with modern C# tools like Visual Studio through the use of 
[source generators](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview).

Plato comes with a set of standard libraries that are useful from any .NET 6.0 projects. 

## Why Plato Exists

Many people are becoming aware of the benefits of pure functional programming practices such as 
eliminating side-effects and using immutable data structures. 

A challenge with mixing pure functional code with imperative code, is that certain guarantees about 
properties of code cannot be made. This limits the effectiveness of tools like optimizers, compilers, 
and analyzers. 

When a system can guarantee that the same output will always be produced reliably for a given set of 
inputs, much more interesting optimizations can be performed. 

Because Plato is a smaller language it makes certain tools, like transpilers (tools that translate from one language 
into another, e.g., Plato to JavaScript) significantly easier to implement. 


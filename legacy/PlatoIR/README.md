# PlatoIR

This is the form designed for: analysis, rewriting, output, evaluation, and partial evaluation. 

From this version we should be able:

1. Generate Plato 
2. Generate JavaScript
3. Generate GLSL
4. Generate Ara gsraphs
5. Generate addition required C# code 
6. Generate tests
7. Generate documentation 

# TODO

The IR representation should be written in Plato.

# NOTES:

The followGenerated code.

* Equals
* operator == / !+
* Create function 
* Default 
* With ... 
* Constructor 
* GetHashCode
* implict conversion to/from Value Type
* ToString / FromString
* ToJSON / FromJSON
* ToBinary / FromBinary
* ToSpan / FromSpan
* deconstructor
* Size  

If: 

* IVector<ScalarT, ValueT>
	* Operators +,-,*,/ etc.
	* IArray implementation 
* IComparable<T>
	* Operators <=, >=, <, >
* IArithmetic 
	* Operators +, -, *, /

# Optimizations to-do:

Reporting
* disassembly output
* memory consumption
* executable size
* cold start-up

Rewriting
* Fields versus properties
* variables over structs
* inlining

Output
* var over type-declaration
* Structs versus classes
* https://docs.microsoft.com/en-us/dotnet/csharp/write-safe-efficient-code#declare-immutable-structs-as-readonly
* https://docs.microsoft.com/en-us/dotnet/csharp/write-safe-efficient-code#use-ref-struct-types
* https://docs.microsoft.com/en-us/dotnet/csharp/write-safe-efficient-code#use-ref-readonly-return-statements
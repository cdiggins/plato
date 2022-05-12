# NOTE:

* Compute face normals. 
* Profile the code in C#. 
* Output the results to a file for test purposes
* Profile the code after Plato to C# 
* Compare the results 
* Automate the generation of the code. 
* Automate the compilation. 
* Test generating code from a regular environement
* Write RoslynUtils 
* Consider using the output window which 

Experiment with different outputs.

1. structs versus classes
2. ref readonly struct 
3. fields versus properties
4. lambda => static 
5. inlining

// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1645 (21H2)
Intel Core i5-1035G1 CPU 1.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.202
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT
  DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT


| Method |     Mean |    Error |   StdDev |
|------- |---------:|---------:|---------:|
|  Torus | 71.45 ms | 1.424 ms | 2.175 ms |



    * DONE: too many constructors
    * DONE: missing parameter types
    * DONE: names not becoming references
    * DONE: some classes missing names.
    * DONE: fix extra indent after first line
    * DONE: need more declaration information
    * DONE: types are missing the names
    * DONE: need to output type parameters for functions and classes
    * DONE: references are unresolved for fields (and probably more things)
    * DONE: need to have the proper name for the ".ctor"     
    * DONE: don't have a line break in the class declaration
    * DONE: don't output the method and getter bodies for interfaces.
    * DONE: I am missing type arguments, particularly when working with built-in types
    * DONE: does Void actually work?
    * DONE: does Single work as well?
    * DONE: I need to make my lambdas work.
    * TODO: I would like the operators to be called explicitly
    * TODO: call the conversion operators explicitly.
    * TODO: design the testing program
    * DONE: namespace is missing
    * TODO: I want to have an explicit "this" when it can be added
    * TODO: I want to convert a whole file into C#, and then compile and test the result.
    * DONE: add the parameters (and variable declarations) to the references. 
    * DONE: some parameters aren't being added to the IRBuilder
    * TODO: renamed the "TYPE operator implicit" to "operator implicit TYPE"
    * DONE: output the property getter
    * DONE: output the indexer getter
    * DPME: generate fields when the property requires it.
    * DONE: don't have return statements in constructors.
    * DONE: replace the built-in type names with the correct key words
    * DONE: output the source code 
    * TODO: output the source location
    * DONE: remove the function reference (should only be type reference)
    * DONE: move the type arguments into a reference
    * DONE: add the list of interfaces to generated code
    * DONE: add the inherited class to code
    * TODO: generate JavaScript from the serializer
    * DONE: compile the generated code
    * TODO: report the results
    * TODO: why do "names" exist? I should really be using a refernce.
    * DONE: only allow block statements for method bodies.
    * DONE: type parameters of methods aren't being added as IR items.
    * TODO: output recievers.
    * DONE: resolve type parameters.
    * DONE: some types aren't showing. 
    * DONE: after merging TypeParameterReference with TypeParameter, some types aren't showing.
    * DONE: I now have some "#unknown" tags.
    * DONE: when a function is a parameter it is shown as "unresolved".
    * TODO: In "ToTriangles" I don't get the array declaration
    * TODO: the type of bilt-in arrays is invisible and shows as unresolved.
    * DONE: add public and static (where appropriate) to members.
    * DONE: indexers did not have the correct return type.
    * TODO: I want to start doing some inlining.
    * TODO: I want to try rewriting the code in a variable declaration form (like I'm doing in JavaScript).
    * TODO: I want to try evaluating the IR.
    * DONE: create a test project.
    * TODO: profiling of code nsippets
    * DONE: dont't put blocks for empty getters.
    * TODO: Fix the formatting, it looks awful. 
    * TODO: make the meta-data optional. 
    * DONE: interfaces should not have fields.
    * DONE: indexer body is missing in some cases
    * DONE: assigning to the auto-property needs to be allowed. 
    * DONE: all references to a property as a lvalue need to be replaced with the auto-property.
    * TODO: decide if I should change it on write, or on parse.
    * TODO: create a hidden constructor. 
    * DONE: normalization of the code. 
    * DONE: rewrite all tuples as lvalues. 
    * TODO: handle "this" function calls.
    * TODO: handle "base" function calls
    * TODO: add deconstructable classes (maybe the just need a "Item1" / "Item2" / etc.
    * TODO: the semantic rules should probably be in the constructors of the IR.
    * DONE: add let statement to IR
    * DONE: add compound expression to IR
    * DONE: tuples to expression statements become compound expressions
    * DONE: output let statements
    * TODO: make everything route through the IRBuilder.
    * DONE: get child expressions
    * TODO: remove all setters from ExpressionIR, meaning types is outside.
    * DONE: get all declarations from a statement
    * DONE: output var, when a type reference is not known.
    * TODO: customize output.https://docs.microsoft.com/en-us/cpp/build/formatting-the-output-of-a-custom-build-step-or-build-event?view=msvc-170
    * DONE: extra semicolons on the property
    * DONE: base class is a "var" 
    * DONE: assignment to properties, should be assignment to the field
    * DONE: why are there no fields on Vector2.
    * TODO: add proper namespace output
    * TODO: basic inlining as an option.
    * DONE: implemented interfaces are missing
    * DONE: add this parameters 
    * DONE: arrays initializers are ascrewed up (   r  = new var[]{}; <= var r = new T[self.Count]; }
    * DONE: array types are declarated property 
    * DONE: lambda parameters without types, should just omit the type
    * DONE: make sure static classes are specified as static classes.
    * DONE: don't add "this" when calling a static method.
    * DONE: don't use "System.Void"
    * DONE: don't use return for void functions (e.g. Log)
    * TODO: support the fact that namespaces are shared across files
    * TODO: support namespaces
    * TODO: add compilation unit
    * TODO: support partial types
    * TODO: support nested types 
    */
/*
 * DONE: Indexer(this) properties not generated.
 * DONE: No constructor generated.
 * DONE: No typeDeclaration provided for expressions.
 * DONE: Backing fields for auto-generated properties are missing
 * SKIPPED: Interpolated string not supported
 * DONE: Tuple expressions and tuple assignment 
 * DONE: Cast doesn't work.
 * DONE: Extension functions are translated into member functions. 
 * DONE: Get the list of captured variables
 * DONE: Construcotr returning interface 
 * DONE: Can't find operator methodDeclaration (need to add "OperationDeclaration")
 * SKIPPED: constructor typeDeclaration overloading is not supported (only one constructor is allowed for now)
 * DONE: overloaded operators need to be converted to functions
 * DONE: operators need to be renamed
 * DONE: auto operators don't have a return statement
 * DONE: all field, methodDeclaration, and propertyDeclaration variables need to use a "this"
 * DONE: Missing ids of functionDeclaration references
 * DONE: something odd is happening when declarating a variable and assigning a new expression to it.
 * DONE: anything leabled VariableDeclarator seems to be a problem
 * DONE: operators need to be class qualfieid
 * DONE: extension methods need to be class qualified
 * DONE: add casts to typeDeclaration definition
 * TODO: generic typeDeclaration constructors are missing typeDeclaration parameters
 * TODO: get all of the typeDeclaration refrences and generate the appropriate types in advance.
 * DONE: this.methodDeclaration calls missing ID
 * TODO: need to generate a functionDeclaration for indexing. operator_Subscript
 * DONE: call cast operator functionDeclaration
 * TODO: need to explicitly cast numbers to ints when doing integers 
 * LATER TODO: handle local functions,
 * LATER TODO: anonymous types
 * LATER TODO: anonymous functions
 * LATER TODO: Need MathF functions  
 * LATER TODO: Need Plato built-in functions (Plato.At , Plato.Cast)
 * LATER TODO: reduce number of expression calls
 * LATER TODO: add ids to parameter references 
 * LATER TODO: add ids to local references
 * TODO: separate the functionDeclaration defintion gathering (semantic analysis)
 * TODO: for functionDeclaration and propertyDeclaration definition creation use the PlatoSyntax objects 
 * TODO: get resharper
 * TODO: find a way to better track my to-do list
 * TODO: generate output from the intermediate representation
 * TODO: have a better way of reporting errors 
 * TODO: a tool for debugging the structure (syntax and semantics)
 * DONE: the indexer will not have an IMethodSymbol, this could cause some problems. 
 * TODO: JavaScriptGenerator is too big.
 * DONE: I need to replace a token in a file, for the generated JavaScript 
 * DONE: I need to create the input file
 * TODO: I need to automatically launch the web-browser
 * TODO: add call when casting floats to ints 
 * DONE: handle swithc expressions
 * TODO: test switch expression code generation
 * DONE: add clone functions
 * DONE: simplify declartions
 * DONE: stack overflow
 * DONE: cloning doesn't back-patch references.
 * DONE: replace clone with rewrite
 * DONE: inlined lambdas need to be expanded (or surrounded with paranthesis)
 

* TODO: 
    * captured variables aren't getting rewritten in the lambda ... probably because I don't want to replace the "return statements". 
        * So I am not entering into the lambda. 
        * I need a guard to say when encountering a return statemnt, not in a lambda, replace it. 
    * there is an issue with implicit "this", I think that we need to treat it like a variable declaration. 
        * Basically I need to replace the implicit this, with an explicit this
        * Then when doing the rewriting I need to replace "this" with the reciever. 
    * There might be an issue with the type. 
    * I am going to have a problem in the futujre because 
   

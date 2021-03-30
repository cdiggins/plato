# Jan 24

* I belive I can many checks by writnbg a C# analyzer, that can transform C# code into Plato code
    * Replace return statments
    * prevent "void" functions 
    * prevent "private", "protected", "abstract", "virtual", "internal" => replace with "public"
    * prevent "for" loops => replace with 
    * assure static is used where it is expected 
        * this is questionable: I'm not sure the programmer should be required to declare whether a function is static
        * They should just be able to use it. 
    * prevent setters => replace with "Set" functions that create a copy. 
    * assure that fields aren't used 
    * enforcing uniqueness of unique types
    * Making "mutable" syntax possible. (x.SetY(42) => x = x.SetY(42))
        * This might require that I label functions with a special annotation, so that the system knows to "rewrite" those function
    * see: 
        * https://devblogs.microsoft.com/dotnet/how-to-write-a-roslyn-analyzer/

* Great example here: 
    * https://stackoverflow.com/questions/11749222/is-there-a-way-to-implement-custom-language-features-in-c

* When something does emulate an assignment, it is more efficient to implement it as an assignment than to implement it as a literal new. 
    * This is a funny challenge: 
        1. start with immutability (that may look like mutation)
        2. make it looks like pure immutability
        3. optimize and rewrite
        4. look for code that can be replaced with actual mutation 

* How do identify "mutation" opportunities?
    * The passed variable is never used again I suppose.  T x; y = f(x); and x is never used again. 

* Plato doesn't require all of the silliness that C# requires:
    * https://docs.microsoft.com/en-us/dotnet/csharp/write-safe-efficient-code
    * Deconstruction comes for free 

* There may be some scenarios where a "code generator" could be useful, but in general it is not a replacemnt for Plato:
    * https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.cookbook.md
    * https://github.com/dotnet/roslyn/blob/master/docs/features/source-generators.md
    * https://devblogs.microsoft.com/dotnet/new-c-source-generator-samples/

Statements that are allowed: 

Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.BreakStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.CommonForEachStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ContinueStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.EmptyStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.LockStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax

Statements that are not allowed in Plato:

Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.YieldStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.UsingStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.FixedStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.FixedStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.LabeledStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.CheckedStatementSyntax

...

More syntax:

Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.AccessorListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ArrayRankSpecifierSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ArrowExpressionClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.AttributeTargetSpecifierSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.BaseArgumentListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.BaseCrefParameterListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.BaseExpressionTypeClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.BaseListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.BaseParameterListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.CatchDeclarationSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.CatchFilterClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorInitializerSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.CrefParameterSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.CrefSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ElseClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionOrPatternSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.FinallyClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringContentSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationAlignmentClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.InterpolationFormatClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.NameColonSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.PositionalPatternClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.PropertyPatternClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.QueryBodySyntax
Microsoft.CodeAnalysis.CSharp.Syntax.QueryClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.SelectOrGroupClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.StructuredTriviaSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.SubpatternSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.SwitchExpressionArmSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.SwitchLabelSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.SwitchSectionSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.TypeArgumentListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterConstraintSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterListSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.TypeParameterSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.VariableDesign
ationSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.WhenClauseSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.XmlAttributeSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementEndTagSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementStartTagSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.XmlNameSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.XmlNodeSyntax
Microsoft.CodeAnalysis.CSharp.Syntax.XmlPrefixSyntax

Roslyn :
* Semantic model :
    * https://joshvarty.com/2014/10/30/learn-roslyn-now-part-7-introducing-the-semantic-model/
    * https://stackoverflow.com/questions/31861762/finding-all-references-to-a-method-with-roslyn
* Data Flow Analysis:
    * https://joshvarty.com/2015/02/05/learn-roslyn-now-part-8-data-flow-analysis/
    * https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.csharpextensions.analyzedataflow?view=roslyn-dotnet
    * https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.dataflowanalysis?view=roslyn-dotnet
* Control Flow Analyis:
    * https://joshvarty.com/2015/03/24/learn-roslyn-now-control-flow-analysis/


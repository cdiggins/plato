using Microsoft.CodeAnalysis;

namespace PlatoAnalyzer
{
    /*
    public static string RemapType(string s)
    {
        switch (s)
        {
            case "Single": return "float";
            case "Int32": return "int";
            case "Boolean": return "bool";
        }
    }*/

    /*
     * TODO:
     * - I need to generate intrinsics for the built-in types.
     * - I don't want to have to explicitly implement each interface: some of them have an obvious implementation.
     *  - A whole bunch of them have automatic implementations. So what do I do?
     *  - What is an example of an automatic implementation:
     *  - To "self.Fubar(other) => new selfType(self.Component.Fubar(other.Component));"
     *  - This applies to a lot of binary operators ... what about Min/Max/Add/Subtract/ etc.
     *  - There are boolean operators (equals etc.), some are "OR" (not equals) some are "AND" (equals).
     *  - How is that specified?
     *  - I could just hard-code the logic.
     * - Intrinsics. 
     */

    [Generator]
    public class PlatoGenerator : ISourceGenerator
    {

        public void Execute(GeneratorExecutionContext context)
        {
            //context.AddSource($"generated.g.cs", "/* no content right now */");
        }

        // https://stackoverflow.com/questions/33420559/how-to-find-type-of-the-field-with-specific-name-in-roslyn

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}

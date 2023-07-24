using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoAst
{

    /// <summary>
    /// Used primarily to figure out what each name means, and what the type of each expression is.
    /// This allows us to figure out which methods will get called at run-time.
    /// An abstract evaluator returns abstract values which have types, scopes, and locations 
    /// </summary>
    public class SymbolResolver
    {
        public SymbolResolver()
        {
            BindPredefinedSymbols();
        }

        public Scope ValueBindingsScope { get; set; } = new Scope(null, null);
        public Scope TypeBindingsScope { get; set; } = new Scope(null, null);
        public Dictionary<Symbol, AstNode> SymbolsToNames = new Dictionary<Symbol, AstNode>();

        public IEnumerable<TypeDefSymbol> TypeDefs => SymbolsToNames.Keys.OfType<TypeDefSymbol>();
        
        public void BindPredefinedSymbols()
        {
            BindPredefined("intrinsic");
            BindPredefined("Tuple");
            BindType("Self", Primitives.Self);
        }

        public void BindPredefined(string name)
        {
            BindValue(name, new PredefinedSymbol(null, name));
        }

        public T BindValue<T>(string name, T value) where T : Symbol
        {
            ValueBindingsScope = ValueBindingsScope.Bind(name, value);
            return value;
        }

        public T BindType<T>(string name, T value) where T : Symbol
        {
            TypeBindingsScope = TypeBindingsScope.Bind(name, value);
            return value;
        }

        public FunctionGroupSymbol BindFunctionToGroup(string name, MemberDefSymbol method)
        {
            var val = ValueBindingsScope.GetValue(name);

            if (val is FunctionGroupSymbol group)
            {
                return BindValue(name, group.Add(method));
            }

            var fgs = new FunctionGroupSymbol(new[] { method }, method.Name);
            return BindValue(name, fgs);
        }

        public RefSymbol GetValue(AstNode location, Scope scope, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Invalid variable name");
            var sym = ValueBindingsScope.GetValue(name);
            if (sym == null)
                throw new Exception($"Could not find symbol {name}");
            if (sym is DefSymbol ds)
                return new RefSymbol(ds);
            throw new NotImplementedException();
        }

        public TypeRefSymbol ResolveType(AstTypeNode astTypeNode)
        {
            if (astTypeNode == null)
                return null;
            var name = astTypeNode.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Invalid variable name");
            var sym = TypeBindingsScope.GetValue(name);
            if (sym == null)
                return null;

            var tds = sym as TypeDefSymbol;

            // TODO: Won't resolve if the type is a parimitive. Will al nameso potneitally conflict with values.
            // I think I need to have type versus value scopes (or name binding). 

            return new TypeRefSymbol(tds, astTypeNode.TypeArguments.Select(ResolveType).ToArray());
        }

        public ParameterSymbol Resolve(AstParameterDeclaration astParameterDeclaration)
        {
            return BindValue(astParameterDeclaration.Name,
                new ParameterSymbol(astParameterDeclaration.Name, 
                    ResolveType(astParameterDeclaration.Type)));
        }

        // This is not a pure function. It updates the scope every time a new value is bound
        public Symbol Resolve(AstNode node)
        {
            if (node == null)
                return null;

            switch (node)
            {
                case AstTypeNode astTypeNode:
                    return ResolveType(astTypeNode);

                case AstAssign astAssign:
                    return BindValue(astAssign.Var,
                        new AssignmentSymbol(
                            GetValue(node, ValueBindingsScope, astAssign.Var),
                            Resolve(astAssign.Value)));

                case AstBlock astBlock:
                {
                    if (astBlock.Statements.Count == 0)
                        return null;
                    if (astBlock.Statements.Count == 1)
                        return Resolve(astBlock.Statements[0]);
                    throw new Exception("Cannot handle AstBlock with multiple children");
                }

                case AstBreak astBreak:
                    return NoValueSymbol;

                case AstMulti astMulti:
                {
                    if (astMulti.Nodes.Count == 0)
                        return null;
                    if (astMulti.Nodes.Count == 1)
                        return Resolve(astMulti.Nodes[0]);
                    throw new Exception("Cannot handle AstMulti with multiple children");
                }

                case AstParenthesized astParenthesized:
                    return Resolve(astParenthesized.Inner);
                
                case AstReturn astReturn:
                    return Resolve(astReturn.Value);

                case AstConditional astConditional:
                    return Scoped(() => new ConditionalExpressionSymbol(
                        Resolve(astConditional.Condition),
                        Resolve(astConditional.IfTrue),
                        Resolve(astConditional.IfFalse)));

                case AstConstant astConstant1:
                    return new LiteralSymbol(astConstant1.Value);

                case AstContinue astContinue:
                    return NoValueSymbol;

                case AstExpressionStatement astExpressionStatement:
                    return Scoped(() => Resolve(astExpressionStatement.Expression));
                    
                case AstIdentifier astIdentifier:
                    return GetValue(node, ValueBindingsScope, astIdentifier.Text);

                case AstInvoke astInvoke:
                {
                    var args = astInvoke.AstArguments.Select((a,i) => 
                        new ArgumentSymbol(Resolve(a), i)).ToArray();
                    var func = Resolve(astInvoke.Function);

                    // TODO: we need to figure out what function it is because really 
                    // evaluating a name will gives us a method group!
                    return new FunctionCallSymbol(func, args);
                }

                case AstLambda astLambda:
                {
                    ValueBindingsScope = ValueBindingsScope.Push();
                    var ps = astLambda.Parameters.Select(Resolve).ToArray();
                    var body = Resolve(astLambda.Body);
                    var r = new FunctionSymbol("__lambda__",
                        Primitives.Lambda.ToRef, body, ps);
                    return r;
                }

                case AstNoop astNoop:
                    return NoValueSymbol;

                case AstVarDef astVarDef:
                    return BindValue(astVarDef.Name, 
                        new VariableSymbol(astVarDef.Name, ResolveType(astVarDef.Type)));

                case AstLoop astLoop:
                    throw new NotImplementedException();
            }

            throw new Exception($"Node can't be evaluated : {node}");
        }

        public void CreateTypeDefs(IEnumerable<AstTypeDeclaration> types)
        {
            // Typedefs and their methods are all at the top-level
            foreach (var astTypeDeclaration in types)
            {
                var typeDef = new TypeDefSymbol(astTypeDeclaration.Kind, astTypeDeclaration.Name);
                SymbolsToNames.Add(typeDef, astTypeDeclaration);
                BindType(typeDef.Name, typeDef);
            }

            foreach (var typeDef in TypeDefs.ToList())
            {
                var astTypeDeclaration = SymbolsToNames[typeDef] as AstTypeDeclaration;

                foreach (var tp in astTypeDeclaration.TypeParameters)
                {
                    var tpd = new TypeParameterDefSymbol(tp.Name);
                    BindType(tpd.Name, tpd);
                    typeDef.TypeParameters.Add(tpd);
                }

                // TODO: I'm not sure about this. It exists because I use the name a s a cosntructor and function.  
                BindValue(typeDef.Name, typeDef);

                foreach (var m in astTypeDeclaration.Members)
                {
                    if (m is AstFieldDeclaration fd)
                    {
                        var fDef = new FieldDefSymbol(typeDef, ResolveType(fd.Type), fd.Name);
                        typeDef.Fields.Add(fDef);
                        SymbolsToNames.Add(fDef, fd);
                        BindFunctionToGroup(fDef.Name, fDef);
                    }
                    else if (m is AstMethodDeclaration md)
                    {
                        var mDef = new MethodDefSymbol(typeDef, ResolveType(md.Type), md.Name);
                        typeDef.Methods.Add(mDef);
                        SymbolsToNames.Add(mDef, md);
                        BindFunctionToGroup(mDef.Name, mDef);
                    }
                    else
                    {
                        throw new NotSupportedException($"MemberDefSymbol not recognized {m}");
                    }
                }

            }

            // Foreach type def create the members 
            foreach (var kv in SymbolsToNames)
            {
                var typeDef = kv.Key as TypeDefSymbol;
                var typeDecl = kv.Value as AstTypeDeclaration;

                if (typeDef == null)
                    continue; 

                ValueBindingsScope = ValueBindingsScope.Push();
                TypeBindingsScope = TypeBindingsScope.Push();

                // TODO: should go into the type scope.
                foreach (var tp in typeDecl.TypeParameters)
                {
                    var tpd = new TypeParameterDefSymbol(tp.Name);
                    BindType(tpd.Name, tpd);
                    typeDef.TypeParameters.Add(tpd);
                }

                // For each method in the type 
                foreach (var method in typeDef.Methods)
                {
                    ValueBindingsScope = ValueBindingsScope.Push();
                    var location = SymbolsToNames[method] as AstMethodDeclaration;
                    var list = new List<ParameterSymbol>();
                    foreach (var p in location.Parameters)
                    {
                        var pValue = Resolve(p);
                        list.Add(pValue);
                    }

                    var body = Resolve(location.Body);
                    var f = new FunctionSymbol(method.Name, method.Type, body, list.ToArray());
                    method.Function = f;
                    ValueBindingsScope = ValueBindingsScope.Pop();
                }

                ValueBindingsScope = ValueBindingsScope.Pop();
                TypeBindingsScope = TypeBindingsScope.Pop();
            }

            // Foreach type add the inherited and the implemented type.
            foreach (var td in TypeDefs)
            {
                var astTypeDecl = SymbolsToNames[td] as AstTypeDeclaration;
                
                foreach (var inheritedType in astTypeDecl.Inherits)
                    td.Inherits.Add(ResolveType(inheritedType));

                foreach (var implementedType in astTypeDecl.Implements)
                    td.Inherits.Add(ResolveType(implementedType));
            }
        }

        public T Scoped<T>(Func<T> f) where T : Symbol
        {
            ValueBindingsScope = ValueBindingsScope.Push();
            var r = f();
            ValueBindingsScope = ValueBindingsScope.Pop();
            return r;
        }

        public static NoValueSymbol NoValueSymbol 
            = NoValueSymbol.Instance;
    }
}

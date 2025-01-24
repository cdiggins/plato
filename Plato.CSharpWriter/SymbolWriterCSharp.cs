﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.Compiler.Analysis;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;


namespace Plato.CSharpWriter
{
    public class SymbolWriterCSharp : CodeBuilder<SymbolWriterCSharp>
    {
        public string FloatType;
        public string Namespace;
        public string SelfType;

#if CHANGE_PRECISION
        public string OtherPrecisionFloatType;
        public string OtherPrecisionNamespace;
#endif

        public Compiler.Compilation Compilation { get; }
        public Dictionary<string, StringBuilder> Files { get; } = new Dictionary<string, StringBuilder>();

        public DirectoryPath OutputFolder { get; }

        public static HashSet<string> IgnoredTypes = new HashSet<string>()
        {
            "Dynamic",
            "Array",
            "Array2D",
            "Array3D",
            "Function0",
            "Function1",
            "Function2",
            "Function3",
        };

        public static HashSet<string> IgnoredFunctions = new HashSet<string>()
        {
            "FieldNames",
            "FieldValues",
            "TypeName",
            "Components",
            "CreateFromComponents",
            "NumComponents",
            "Equals",
            "NotEquals",
            "GetHashCode",
            "ToString",
            "GetType",
        };

        public static Dictionary<string, string> PrimitiveTypes = new Dictionary<string, string>()
        {
            { "Number", "float" },
            { "Boolean", "bool" },
            { "Integer", "int" },
            { "Character", "char" },
            { "String", "string" },
            { "Dynamic", "object" },
            { "Type", "System.Type" },
        };

        public static HashSet<string> IntrinsicTypes = new()
        {
            "Angle", 
            "Matrix3x2", 
            "Matrix4x4", 
            "Quaternion", 
            "Plane", 
            "Vector2", 
            "Vector3", 
            "Vector4", 
            "Vector8"
        };

        public SymbolWriterCSharp(Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            Compilation = compilation;
            OutputFolder = outputFolder;
        }

        public SymbolWriterCSharp WriteFile(FilePath fileName, Func<SymbolWriterCSharp> f)
        {
            StartNewFile(fileName);
            WriteLine($"// Autogenerated file: DO NOT EDIT");
            WriteLine($"// Created on {DateTime.Now}");
            WriteLine();
            WriteLine("using System.Runtime.CompilerServices;");
            WriteLine("using System.Runtime.Serialization;");
            WriteLine("using System.Runtime.InteropServices;");
            WriteLine("using static System.Runtime.CompilerServices.MethodImplOptions;");
            WriteLine("");
            WriteLine($"namespace {Namespace}");
            WriteStartBlock();
            f();
            WriteEndBlock();
            return this;
        }

        public SymbolWriterCSharp WriteAll(string floatType)
        {
            FloatType = floatType;
            Namespace = floatType == "float"
                ? "Plato"
                : floatType == "double"
                    ? "Plato.DoublePrecision"
                    : throw new NotImplementedException("Only 'float' and 'double' are supported");
#if CHANGE_PRECISION
            OtherPrecisionFloatType = floatType == "float" ? "double" : "float";
            OtherPrecisionNamespace = floatType == "float" ? "Plato.DoublePrecision" : "Plato";
#endif 

            WriteFile("Interfaces.g.cs", WriteConceptInterfaces);
            WriteFile("Constants.g.cs", WriteConstantLibraryMethods);
            WriteFile("Extensions.g.cs", WriteInterfaceLibraryMethods);

            foreach (var c in Compilation.ConcreteTypes)
            {
                var name = c.Type.Name;
                if (!IgnoredTypes.Contains(name))
                    WriteFile($"{name}.g.cs", () => WriteTypeImplementation(c));
            }

            return this;
        }

        public SymbolWriterCSharp SetSelfType(string selfType, Func<SymbolWriterCSharp> f)
        {
            var oldSelfType = SelfType;
            try
            {
                SelfType = selfType;
                return f();
            }
            finally
            {
                SelfType = oldSelfType;
            }
        }

        public void WriteAnalysis(FunctionInstance fa, string indent = "  ")
        {
            var parameters = fa.Implementation.Parameters.Select(p => $"{p.Type} {p.Name}").JoinStringsWithComma();
            WriteLine($"{indent}Function: {fa.Implementation.Name}({parameters})");
        }

        public void WriteConceptImplementation(ConceptImplementation ci, string indent = "  ")
        {
            WriteLine($"{indent}Concept={ci.Concept.Name} Expr={ci.Expression} Subs={ci.Substitutions}");
            foreach (var fa in ci.ImplementedFunctions)
                WriteAnalysis(fa, indent);
            foreach (var ci2 in ci.Children)
                WriteConceptImplementation(ci2, indent + "  ");
        }

        public void WriteAnalyses()
        {
            StartNewFile(OutputFolder.RelativeFile("analysis.txt"));
            foreach (var ta in Compilation.ConcreteTypes)
            {
                WriteLine($"Type analysis for {ta.Type.Name}");
                foreach (var ci in ta.Concepts)
                {
                    WriteConceptImplementation(ci);
                }
                foreach (var fa in ta.ConcreteFunctions)
                {
                    WriteAnalysis(fa);
                }
            }
        }

        public bool IsStaticOrLambda { get; set; }

        public SymbolWriterCSharp WriteStaticOrLambdaBody(Symbol sym)
        {
            var oldStaticOrLambda = IsStaticOrLambda;
            IsStaticOrLambda = true; 
            var r = Write(sym);
            r.IsStaticOrLambda = oldStaticOrLambda;
            return r;
        }

        public void StartNewFile(string fileName)
        {
            sb = new StringBuilder();
            Files.Add(fileName, sb);
        }

        public SymbolWriterCSharp Write(IEnumerable<Symbol> symbols)
            => symbols.Aggregate(this, (writer, symbol) => writer.Write(symbol));

        public SymbolWriterCSharp WriteCommaList(IEnumerable<Symbol> symbols) 
            => WriteCommaList(symbols, (w, s) => w.Write(s));

        public SymbolWriterCSharp WriteCommaList(IEnumerable<string> symbols)
            => WriteCommaList(symbols, (w, s) => w.Write(s));

        public SymbolWriterCSharp Write(Symbol symbol, string type = null)
        {
            switch (symbol)
            {
                case TypeDef typeDefinition:
                    return Write(typeDefinition);
                case DefSymbol definition:
                    return Write(definition);
                case Expression expression:
                    return Write(expression, type);
                case Statement statement:
                    return Write(statement);
                case TypeExpression typeExpression:
                    return Write(typeExpression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol));
            }
        }

        public SymbolWriterCSharp WriteBody(FunctionInfo fi, bool isStatic)
        {
            var oldStaticOrLambda = IsStaticOrLambda;
            IsStaticOrLambda = isStatic;

            if (fi.Body == null)
                return WriteLine(" => throw new NotImplementedException();");

            var isProp = isStatic ? fi.NumParameters == 0 : fi.NumParameters == 1;
            var body = fi.Body?.RewriteLambdasCapturingVars();
            if (isProp)
                Write($" {{ {Annotation} get ");
            if (body is Expression)
                Write($" => ").Write(body, fi.ReturnType).Write(";");
            else if (body is BlockStatement)
                Write(body);
            if (isProp)
                Write(" } ");
            WriteLine();

            IsStaticOrLambda = oldStaticOrLambda;
            return this;
        }

        public SymbolWriterCSharp GenerateFunc(FunctionInfo f, ConcreteType t)
        {
            var pns = f.ParameterNames;
            var fs = t.Type.Fields.Select(tf => tf.Name).ToList();
            Write(f.MethodSignature);

            if (f.Name == "At")
            {
                var p = pns[1];
                var s = "";
                for (var i=0; i < fs.Count; i++)
                    s += $"{p} == {i} ? {fs[i]} : ";
                s += $"throw new System.IndexOutOfRangeException()";
                WriteLine($" => {s};");
                WriteLine($"{f.IndexerSig} {{ {Annotation} get => At(n); }}"); 
                return this;
            }

            if (f.Name == "Count")
            {
                return WriteLine($" {{ {Annotation} get => {fs.Count}; }}");
            }

            throw new Exception("Only 'At' or 'Count' supported");
        }

        public string ToCSharp(TypeExpression expr)
            => ToCSharp(TypeInstance.Create(expr), SelfType);

        public string ToCSharp(TypeInstance type)
            => ToCSharp(type, SelfType);

        public string ToCSharp(TypeDef type)
            => ToCSharp(type.ToTypeExpression(), SelfType);

        public static string ToCSharp(TypeExpression expr, string selfType)
            => ToCSharp(TypeInstance.Create(expr), selfType);

        public static string ToCSharp(TypeInstance type, string selfType)
        {
            var sb = new StringBuilder();
            if (type.Name == "Self" && selfType != null)
                return selfType;

            if (type.Name.StartsWith("Function"))
                sb.Append("System.Func");
            else
                sb.Append(type.Name);

            if (type.Args.Count > 0)
                sb.Append("<")
                    .Append(string.Join(", ", type.Args.Select(a => ToCSharp(a, selfType))))
                    .Append(">");

            return sb.ToString();
        }
        
        public SymbolWriterCSharp WriteMemberFunction(FunctionInfo f, bool isPrimitive)
        {
            if (!isPrimitive || f.Body != null)
            {
                Write(f.MethodSignature);
                WriteBody(f, false);
            }
            else
            {
                if (f.IsOperator)
                {
                    Write(f.MethodSignature);
                    
                    if (f.ParameterNames.Count == 1)
                    {
                        WriteLine($" {{ {Annotation} get => {f.OperatorName}this; }}");
                    }
                    else if (f.ParameterNames.Count == 2)
                    {
                        WriteLine($" => this {f.OperatorName} {f.ParameterNames[1]};");
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                return this;
            }

            if (f.IsOperator && !isPrimitive)
                WriteLine(f.OperatorImpl);

            if (f.IsIndexer)
                WriteLine(f.IndexerImpl);

            if (f.IsImplicit)
            {
                if (f.ConcreteType.Name == f.Name)
                    Debug.WriteLine("Skipping implicit cast to self");
                else if (f.ConcreteType.Type.Fields.Count == 1 && f.ConcreteType.Type.Fields[0].Type.Def.Name == f.Name)
                    Debug.WriteLine("Skipping implicit cast to single field (already included)");
                else
                    WriteLine(f.ImplicitImpl);
            }

            return this;
        }
        
        public SymbolWriterCSharp WriteStaticFunction(FunctionInfo fi)
            => Write($"{fi.StaticSignature}").WriteBody(fi, true);

        public SymbolWriterCSharp WriteExtensionFunction(FunctionInfo fi)
            => Write($"{fi.ExtensionSignature}").WriteBody(fi, true);

        public SymbolWriterCSharp WriteConstantLibraryMethods()
        {
            WriteLine($"public static class Constants");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllConstants())
                WriteStaticFunction(ToFunctionInfo(f, null, FunctionInstanceKind.Constant));
            WriteEndBlock();
            return this;
        }

        public SymbolWriterCSharp WriteInterfaceLibraryMethods()
        {
            WriteLine($"public static class Extensions");
            WriteStartBlock();
            foreach (var f in Compilation.Libraries.AllFunctions())
            {
                if (f.NumParameters > 0)
                {
                    var pt = f.Parameters[0].Type;
                    if (!pt.Def.IsConcept())
                        continue;

                    // TODO: this is a HACK! we are temporarily only enabling this for IArray. 
                    // Ultimately it needs to be done with Self-constrained versions of the interfaces. 
                    // Writing those function signatures will be a lot of work. 
                    // Even then, there could be some problems (like 
                    if (!pt.Def.Name.StartsWith("IArray")) 
                        continue; 

                    var fi = ToFunctionInfo(f, null, FunctionInstanceKind.InterfaceExtension);
                    WriteExtensionFunction(fi);
                }
            }
            WriteEndBlock();
            return this;
        }
        
        public SymbolWriterCSharp WriteConceptInterfaces()
        {
            foreach (var c in Compilation.AllTypeAndLibraryDefinitions)
                if (c.IsConcept())
                    WriteConceptInterface(c);
            return this;
        }

        public static string JoinTypeParameters(IEnumerable<string> parameters)
        {
            var r = parameters.JoinStrings(", ");
            return r.Length == 0 ? r : $"<{r}>";
        }

        public string TypeStr(TypeExpression type)
        {
            var typeArgs = type.TypeArgs;
            return type.Name + JoinTypeParameters(typeArgs.Select(TypeStr));
        }

        public string TypeAsInherited(TypeExpression type, bool useSelf = true)
        {
            var typeArgs = type.TypeArgs;
            return type.Name + JoinTypeParameters(type.Def.IsSelfConstrained() && useSelf 
                ? typeArgs.Select(TypeStr).Prepend("Self") 
                : typeArgs.Select(TypeStr));
        }

        public SymbolWriterCSharp WriteConceptInterfaceFunctions(TypeDef type)
        {
            //var t = ToCSharp(type);
            WriteStartBlock();
            foreach (var m in type.Methods)
            {
                var fi = ToFunctionInfo(m.Function, null, FunctionInstanceKind.ConceptInterface);
                if (fi.IsStatic)
                    continue;
                WriteLine(fi.MethodInterface);
                if (fi.IsIndexer)
                    WriteLine(fi.IndexerInterface);
            }

            WriteEndBlock();
            return this; 
        }

        public SymbolWriterCSharp WriteConceptInterface(TypeDef type)
        {
            // There may be one or two interfaces. 
            // If the type is self-constrained, then a version of the interface is created that has an explicit "Self" type as the first argument. 
            // And another version is created that does not: this is called the "simple interface".
            // The self constrained interface is more precise and efficient: it describes exactly the types that come in and out of the functions. 
            // The simple interface required dynamic casting to be made to work. 

            Debug.Assert(type.IsConcept());

            // We have a special implementation of IArray
            if (type.Name.StartsWith("IArray"))
                return this;

            var baseTypeParams = type.TypeParameters.Select(tp => tp.Name).ToList();

            var typeParams = type.IsSelfConstrained()
                ? baseTypeParams.Prepend("Self")
                : baseTypeParams;

            var baseFullName = $"{type.Name}{JoinTypeParameters(baseTypeParams)}";
            var fullName = $"{type.Name}{JoinTypeParameters(typeParams)}";

            
            var inherits = type.Inherits.Select(t => TypeAsInherited(t, true)).ToList();
            
            // TEMP: removing "simple" interfaces
            //if (type.IsSelfConstrained())
            //    inherits.Add(baseFullName);
            
            var inherited = inherits.Count > 0 
                ? ": " + inherits.JoinStringsWithComma() : "";

            Write("public interface ").Write(fullName).WriteLine(inherited);

            foreach (var tp in type.TypeParameters)
            {
                foreach (var constraint in tp.Constraints)
                {
                    if (constraint.Name == "IAny")
                        continue;
                    var constraintArgs = JoinTypeParameters(constraint.Def.IsSelfConstrained()
                        ? constraint.TypeArgs.Select(TypeStr).Prepend(tp.Name)
                        : constraint.TypeArgs.Select(TypeStr));
                    WriteLine($"where {tp.Name} : {constraint.Name}{constraintArgs}");
                }
            }

            WriteConceptInterfaceFunctions(type);
            return this;
        }

        public string ImplementedTypeString(TypeExpression te, string typeName, bool useSelf)
        {
            var typeArgs = te.TypeArgs.Select(TypeStr);
            if (te.Def.IsSelfConstrained() && useSelf)
                typeArgs = typeArgs.Prepend(typeName);
            return $"{te.Name}{JoinTypeParameters(typeArgs)}";
        }

        public static string FieldNameToParameterName(string fieldName)
            => fieldName.Length == 0 || fieldName[0].IsLower() 
                ? $"_{fieldName}" 
                : fieldName.DecapitalizeFirst();

        public static string Annotation => $"[MethodImpl(AggressiveInlining)]";
        
        public SymbolWriterCSharp WriteTypeImplementation(ConcreteType concreteType)
        {
            return new ConcreteTypeWriter(Compilation, concreteType).Write(this);
        }

        public SymbolWriterCSharp WriteSimpleInterface(ConcreteType concrete, string name, TypeExpression concept)
        {
            if (!concept.Def.IsSelfConstrained())
                return this; 

            var its = ImplementedTypeString(concept, concrete.Type.Name, false);

            return SetSelfType(its, () =>
            {
                WriteLine($"// Implementation of {concept.Name}");
                foreach (var f in concept.Def.Functions)
                {
                    var ret = ToCSharp(concept.GetReplacement(f.ReturnType));
                    var pts = f.Parameters.Skip(1).Select(p => ToCSharp(concept.GetReplacement(p.Type))).ToList();
                    var pns = f.Parameters.Skip(1).Select(p => p.Name).ToList();
                    var ps = pts.Zip(pns, (pt, pn) => $"{pt} {pn}").JoinStringsWithComma();
                    if (ps.Length > 0) ps = $"({ps})";
                    var casts = pts.Select(p => p == its ? name : p).ToList();
                    var args = casts.Zip(pns, (cast, pn) => $"({cast}){pn}").JoinStringsWithComma();
                    if (args.Length > 0) args = $"({args})";
                    WriteLine($"{ret} {its}.{f.Name}{ps} => this.{f.Name}{args};");
                }

                return this;
            });
        }

        public int ScoreFunction(FunctionInstance f)
        {
            var a = f.ConcreteType;
            var b = f.Concept;

            // Intrinsics are always preferred 
            if (f.Implementation.OwnerType.Name == "Intrinsics")
                return -200;

            // We assume that if there is no concept, then the function implementation originated as a concrete type.
            // Concrete types provide a better score than any concept. 
            if (b == null)
                return -100;

            var depth = a.Type.DepthTo(b);
            if (depth < 0)
                throw new Exception($"Expected {b} to be a base type of {a}");
            return depth;
        }

        public FunctionInstance ChooseBestFunction(IReadOnlyList<FunctionInstance> xs)
        {
            // We only want distinct implementations. 
            var first = xs[0];
            xs = xs.Distinct(x => x.Implementation.Id).ToList();
            if (xs.Count == 1)
                return xs[0];
            if (xs.Count == 0)
                throw new Exception("No results: could not find a best function.");

            var groups = xs.GroupBy(ScoreFunction).OrderBy(g => g.Key).ToList();
            var group0 = groups[0].ToList();
            Debug.Assert(group0.Count > 0);
            
            if (group0.Count > 1)
                throw new Exception($"// Ambiguous: could not choose a best function implementation for {first}.");

            return group0[0];
        }

        public FunctionInfo ToFunctionInfo(FunctionDef fd, ConcreteType ct, FunctionInstanceKind kind)
            => ToFunctionInfo(new FunctionInstance(fd, ct, null, kind));

        public FunctionInfo ToFunctionInfo(FunctionInstance fi, ConcreteType ct = null)
        {
            var ret = ToCSharp(fi.ReturnType);
            var parameterTypes = fi.ParameterTypes.Select(ToCSharp).ToList();
            var funcTypeParams = ct == null 
                ? fi.UsedTypeParameters
                : fi.UsedTypeParameters.Where(tp => !ct.Type.TypeParameters.Contains(tp)).ToList();

            return new FunctionInfo(fi.Name,
                ct,
                ret,
                funcTypeParams.Select(ToCSharp),
                fi.ParameterNames,
                parameterTypes,
                fi.Implementation.Body,
                fi.IsImplicitCast);
        }

        public FunctionInfo ToFunctionInfoReplaceInterface(FunctionInstance fi, ConcreteType ct, TypeExpression srcType, TypeDef newType)
        {

            var newTypeInstance = TypeInstance.Create(newType.ToTypeExpression());
            var tmp = fi.ParameterTypes.Select(t => t.Expr.IsImplementing(srcType) ? newTypeInstance : t);
            var parameterTypes = tmp.Select(ToCSharp).ToList();
            var ret = ToCSharp(fi.ReturnType.Expr.IsImplementing(srcType) ? newTypeInstance : fi.ReturnType);

            var funcTypeParams = ct == null
                ? fi.UsedTypeParameters
                : fi.UsedTypeParameters.Where(tp => !ct.Type.TypeParameters.Contains(tp)).ToList();

            return new FunctionInfo(fi.Name,
                ct,
                ret,
                funcTypeParams.Select(ToCSharp),
                fi.ParameterNames,
                parameterTypes,
                fi.Implementation.Body,
                fi.IsImplicitCast);
        }

        public static string ParameterizedTypeName(string name, IEnumerable<string> args)
            => name + TypeArgsString(args);

        public static string TypeArgsString(IEnumerable<string> args)
            => args.Any() ? "<" + string.Join(", ", args) + ">" : "";

        public FilePath ToFileName(TypeDef type)
            => OutputFolder.RelativeFile($"{type.Kind}_{type.Name}.cs");
        
        public static void GatherTypeParameters(TypeExpression te, List<string> set)
        {
            if (te.Def.IsSelfConstrained())
                set.Add("Self");
            if (te.Name.StartsWith("$"))
                set.Add("T" + te.Name.Substring(1));
            foreach (var arg in te.TypeArgs)
                GatherTypeParameters(arg, set);
        }

        public static IEnumerable<string> GatherTypeParameters(FunctionDef fd)
        {
            var r = new List<string>();
            foreach (var param in fd.Parameters)
                GatherTypeParameters(param.Type, r);
            return r;
        }

        public SymbolWriterCSharp Write(DefSymbol value)
        {
            switch (value)
            {
                case null:
                    return Write("null");

                case FieldDef fieldDef:
                    return Write("public ").Write(fieldDef.Type).Write(fieldDef.Name).Write(" { get; }")
                        .WriteLine();

                case FunctionDef function:
                    throw new Exception("Not implemented");

                case FunctionGroupDef memberGroup:
                    return Write(memberGroup.Name);

                case MethodDef methodDef:
                    throw new Exception("Not implemented");

                case MemberDef member:
                    throw new Exception("Not implemented");

                case ParameterDef parameter:
                    return Write(parameter.Type).Write(parameter.Name);

                case VariableDef variable:
                    return Write("var ").Write(variable.Name).Write(" = ").Write(variable.Value).WriteLine(";");
            }

            return this;
        }

        public SymbolWriterCSharp Write(TypeExpression typeExpression) 
            => Write(TypeStr(typeExpression)).Write(" ");

        public static string GetLiteralType(Literal literal) 
            => literal.TypeEnum.ToString();

        public static string GetLiteralValue(Literal literal) 
            => literal.Value.ToLiteralString();

        public SymbolWriterCSharp Write(Statement st)
        {
            switch (st)
            {
                case ReturnStatement returnSymbol:
                    Write("return ");
                    if (returnSymbol.Expression != null)
                        Write(returnSymbol.Expression);
                    WriteLine(";");
                    return this;
                
                case LoopStatement loopSymbol:
                    Write("while (").Write(loopSymbol.Condition).WriteLine(")");
                    WriteStartBlock();
                    Write(loopSymbol.Body);
                    WriteEndBlock();
                    WriteLine();
                    return this;

                case MultiStatement multiStatement:
                    foreach (var child in multiStatement.Symbols)
                    {
                        Write(child);
                        if (child is Expression)
                            WriteLine(";");
                    }
                    return this;

                case BlockStatement block:
                {
                    WriteStartBlock();
                    foreach (var x in block.Symbols)
                    {
                        Write(x);
                        if (x is Expression)
                            WriteLine(";");
                    }

                    return WriteEndBlock();
                }

                case CommentStatement commentStatement:
                    return Write($"/* {commentStatement.Comment} */");

                case IfStatement ifStatement:
                    Write("if (");
                    Write(ifStatement.Condition);
                    WriteLine(")");
                    Write(ifStatement.IfTrue);
                    if (ifStatement.IfFalse != null)
                    {
                        WriteLine("else");
                        Write(ifStatement.IfFalse);
                    }

                    return this;
                }
                    
            return this;
        }

        public SymbolWriterCSharp WriteFunctionCall(FunctionCall functionCall)
        {
            // If there are no arguments, it is a constant.
            if (functionCall.Args.Count == 0)
                return Write("Constants.").Write(functionCall.Function);

            // Calling a parameter, or variable 
            if (functionCall.Function is ParameterRefSymbol 
                || functionCall.Function is VariableRefSymbol
                || functionCall.Function is FunctionCall)
            {
                return Write(functionCall.Function).Write(".Invoke(").WriteCommaList(functionCall.Args).Write(")");
            }

            var f = functionCall.Function;
            if (f.Name.StartsWith("Tuple"))
            {
                // To-do: change to a "new". 
                //Debug.WriteLine("Tuple");
                
                return Write("(").WriteCommaList(functionCall.Args).Write(")");
            }

            var arg = functionCall.Args[0];
            Write(arg).Write(".").Write(functionCall.Function);
            
            if (functionCall.Args.Count == 1 && !functionCall.HasArgList) 
                return this;
            
            return Write("(").WriteCommaList(functionCall.Args.Skip(1)).Write(")");
        }

        public SymbolWriterCSharp Write(Expression expr, string type = null)
        {
            if (expr == null)
                return this;    
            
            switch (expr)
            {
                case NewExpression newExpression:
                    return Write($"new {newExpression.Type}(").WriteCommaList(newExpression.Args).Write(")");

                case ParameterRefSymbol pr:
                    return pr.Def.Index == 0 && !IsStaticOrLambda
                        ? Write("this") 
                        : Write(pr.Name);

                case FunctionGroupRefSymbol fgr:
                    // HACK: check if it is a constant.
                    // TODO: I need to have all function     calls properly resolved to generate better quality code. 
                    if (fgr.Def.Functions.Count == 1 &&
                        fgr.Def.Functions[0].NumParameters == 0)
                        return Write($"Constants.{fgr.Name}");
                    return Write(fgr.Name);

                case RefSymbol refSymbol:
                    return Write(refSymbol.Name == "Self" 
                        ? SelfType 
                        : refSymbol.Name);
                    
                case Assignment assignment:
                    return Write(assignment.LValue)
                        .Write(" = ")
                        .Write(assignment.RValue);

                case ConditionalExpression conditional:
                    return Write(conditional.Condition)
                        .Write(" ? ")
                        .Write(conditional.IfTrue)
                        .Write(" : ")
                        .Write(conditional.IfFalse);

                case FunctionCall functionCall:
                    return WriteFunctionCall(functionCall);

                case Literal literal:
                    // TODO: once validating that the cost is superfluous, we can remove this. 
                    return Write($"(({GetLiteralType(literal)}){GetLiteralValue(literal)})");
                    //if (GetLiteralType(literal) == "Number")
                    //    return Write($"{GetLiteralValue(literal)}f");
                    //else
                    //    return Write($"{GetLiteralValue(literal)}");

                case Lambda lambda:
                    return Write("(")
                        .WriteCommaList(lambda.Function.Parameters.Select(p => p.Name))
                        .Write(") => ")
                        .WriteStaticOrLambdaBody(lambda.Function.Body);

                case ArrayLiteral arrayLiteral:
                {
                    var arg = "";
                    if (type != null && type.StartsWith("IArray"))
                        arg = type.Substring("IArray".Length);
                    return Write($"Intrinsics.MakeArray{arg}(")
                        .WriteCommaList(arrayLiteral.Expressions)
                        .Write(")");
                }
            }

            throw new ArgumentOutOfRangeException(nameof(expr));
        }
    }
}
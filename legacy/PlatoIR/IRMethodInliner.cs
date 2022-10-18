using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoIR
{

    public class IRMethodInliner
    {
        public Dictionary<MethodDeclarationIR, MethodDeclarationIR> Inlined =
            new Dictionary<MethodDeclarationIR, MethodDeclarationIR>();

        public static int VarId = 0;

        public static IR ReplaceParametersAndReturnStatements(IR ir, Dictionary<string, ExpressionIR> replacements, Dictionary<string, TypeReferenceIR> typeReplacements, VariableDeclarationIR resultVar, bool inLambda = false)
        {
            if (ir is ParameterReferenceIR pir)
            {
                var pDecl = pir.ParameterDeclaration;
                if (pDecl != null && replacements.ContainsKey(pDecl.ToString()))
                {
                    return replacements[pDecl.ToString()];
                }
            }

            if (ir is ThisIR)
            {
                if (replacements.ContainsKey("this"))
                {
                    return replacements["this"];
                }
            }

            if (ir is TypeReferenceIR tir)
            {
                if (tir.TypeParameterDeclaration != null && typeReplacements.ContainsKey(tir.TypeParameterDeclaration.ToString()))
                {
                    return typeReplacements[tir.TypeParameterDeclaration.ToString()];
                }
            }

            if (!inLambda && ir is ReturnStatementIR rst)
            {
                return new ExpressionStatementIR(
                    new AssignmentIR(new VariableReferenceIR(resultVar.Name, resultVar),
                        rst.Expression.Rewrite(expr =>
                            ReplaceParametersAndReturnStatements(expr, replacements, typeReplacements, resultVar))));
            }

            if (ir is LambdaIR lambdaIr)
            {
                Func<IR, IR> func2 = x =>
                    ReplaceParametersAndReturnStatements(x, replacements, typeReplacements, resultVar, true);

                return new LambdaIR()
                {
                    Body = lambdaIr.Body.Rewrite(func2),
                    Parameters = lambdaIr.Parameters.Rewrite(func2).ToList(),
                };
            }

            return ir;
        }

        public (MultiStatementIR, VariableDeclarationIR) InvocationToStatement(InvocationIR invocation)
        {
            var methodRef = invocation.Function as MethodReferenceIR;
            var lambda = invocation.Function as LambdaIR;
            var method = GetOrComputeInlined(methodRef?.MethodDeclaration);            
            var parameters = method?.Parameters ?? lambda?.Parameters ?? new List<ParameterDeclarationIR>();
            var typeArgs = methodRef?.TypeArguments ?? new List<TypeReferenceIR>();
            var typeParameters = method?.TypeParameters ?? new List<TypeParameterDeclarationIR>();
            var body = method?.Body ?? lambda?.Body;

            if (body == null)
                return (null, null);

            var resultVar = new VariableDeclarationIR()
            {
                InitialValue = new DefaultIR(invocation.ExpressionType.Clone()),
                IsStatic = false,
                Name = $"result_{VarId}",
                Type = invocation.ExpressionType.Clone(),
            };
            VarId++;
            var r = new MultiStatementIR();

            r.Statements.Add(new DeclarationStatementIR(resultVar));

            var block = new BlockStatementIR();
            var parameterReplacements = new Dictionary<string, ExpressionIR>();
            var typeReplacements = new Dictionary<string, TypeReferenceIR>();

            var args = invocation.Args.ToList();
            if (parameters.Count > 0)
            {
                if (parameters[0].IsThisParameter && methodRef?.Receiver != null)
                {
                    args.Insert(0, methodRef.Receiver);
                }
            }

            // Are we calling a memeber function, if so we are going to need to replace the "this"
            if (methodRef?.MethodDeclaration?.IsStatic != false)
            {
                if (methodRef?.Receiver != null)
                {
                    parameterReplacements["this"] = methodRef.Receiver;
                }
            }

            for (var i = 0; i < typeArgs.Count; ++i)
            {
                var typeRef = typeArgs[i];
                var typeParam = typeParameters[i];
                typeReplacements.Add(typeParam.ToString(), typeRef);
            }

            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];

                var arg = i < args.Count ? args[i] : p.DefaultValue;
                if (arg == null)
                {
                    throw new Exception(
                        $"Could not find the correct argument for parameter {p} as position {i} of method {method}");
                }

                if (arg is InvocationIR)
                {
                    var decl = new VariableDeclarationIR()
                    {
                        Type = arg.ExpressionType,
                        Name = $"{p.Name}_{VarId}",
                        InitialValue = arg,
                    };
                    VarId++;
                    parameterReplacements.Add(p.ToString(), decl.ToReference());
                    block.Statements.Add(new DeclarationStatementIR(decl));
                }
                else
                {
                    parameterReplacements.Add(p.ToString(), arg);
                }
            }

            body = body.Rewrite(ir =>
                    ReplaceParametersAndReturnStatements(ir, parameterReplacements, typeReplacements, resultVar),
                    new Dictionary<DeclarationIR, DeclarationIR>());

            block.Statements.Add(body);
            r.Statements.Add(block);

            var replaceString1 = string.Join(Environment.NewLine, parameterReplacements.Select(kv => $"{kv.Key} <- {kv.Value}"));
            var replaceString2 = string.Join(Environment.NewLine, typeReplacements.Select(kv => $"{kv.Key} <- {kv.Value}"));
            resultVar.Source = $"Inlining {invocation} {Environment.NewLine}parameters: {replaceString1} {Environment.NewLine}types: {replaceString2}";

            return (r, resultVar);
        }

        public IR InlineInvocations(IR ir)
        {
            if (ir is StatementIR st)
            {
                var newStatements = new List<StatementIR>();
                var lookup = new Dictionary<InvocationIR, VariableReferenceIR>();
                foreach (var invocation in st.GetExpressions().OfType<InvocationIR>())
                {
                    var (newStatement, resultVar) = InvocationToStatement(invocation);
                    if (newStatement == null || resultVar == null) continue;
                    lookup.Add(invocation, resultVar.ToReference());
                    newStatements.Add(newStatement);
                }

                if (lookup.Count == 0)
                    return ir;

                newStatements.Add((StatementIR)st.Replace(lookup));
                var result = new MultiStatementIR(newStatements);

                // If there are more invocations to inline, then this should do it. 
                return result.Rewrite(InlineInvocations);
            }

            return ir;
        }

        public MethodDeclarationIR GetOrComputeInlined(MethodDeclarationIR original)
        {
            if (original == null) return null;
            // TODO: I don't understand how this happens
            if (original.Name.StartsWith("_inlined_")) return original;
            if (original is OperationDeclarationIR) return original;
            if (Inlined.ContainsKey(original)) return Inlined[original];
            Inlined.Add(original, original);
            var result = original.Rewrite(InlineInvocations);
            result.Name = $"_inlined_{original.Name}";
            Inlined[original] = result;
            return result;
        }

        public IEnumerable<MethodDeclarationIR> GetInlinedMethods()
            => Inlined.Values.Where(f => f.Name.StartsWith("_inlined_"));
    }
}
using System;
using System.Collections.Generic;

namespace PlatoAnalyzer
{
    public class SemanticFactory
    {
        public PlatoSemanticMapping Mapping { get; }

        public Dictionary<ISemantic, int> PlatoNodeIdLookup = new Dictionary<ISemantic, int>();
        public Dictionary<int, ISemantic> SemanticLookup = new Dictionary<int, ISemantic>();

        public SemanticFactory(PlatoSemanticMapping mapping)
        {
            Mapping = mapping;
            CreateDefs();
            CreateRefs();
        }

        public T Create<T>(ISemantic parent, PlatoSyntaxNode node)
            where T: ISemantic, new()
        {
            if (node == null)
                throw new ArgumentNullException(nameof(parent));
            var r = new T
            {
                Parent = parent
            };
            PlatoNodeIdLookup.Add(r, node.Id);
            SemanticLookup.Add(node.Id, r);
            return r;
        }

        public ITypeRef Create(ISemantic parent, PlatoTypeExpr type)
        {
            var r = Create<TypeRef>(parent, type);
            foreach (var arg in type.TypeArguments)
            {
                r.TypeArgs.Add(Create(r, arg));
            }
            return r;
        }

        public IParameter Create(ISemantic parent, PlatoParameter p)
        {
            var r = Create<Parameter>(parent, p);
            r.Type = Create(r, p.Type);
            r.Name = p.Name;
            return r;
        }

        public ITypeParam Create(ISemantic parent, PlatoTypeParam param)
        {
            var r = Create<TypeParam>(parent, param);
            r.Name = param.Name;
            return r;
        }

        public IFunctionDef Create(ISemantic parent, PlatoFunction f)
        {
            var r = Create<FunctionDef>(parent, f);
            r.Name = f.Name;
            r.ReturnType = Create(r, f.ReturnType);
            r.ReceiverType = Create(r, f.ReceiverType);
            r.Body = Create(r, f.Body);
            foreach (var p in f.Parameters.Parameters)
            {
                r.Parameters.Add(Create(r, p));
            }
            foreach (var t in f.TypeParameters.TypeParameters)
            {
                r.TypeParams.Add(Create(r, t));
            }
            return r;
        }

        /*
        public IDef ResolveRef(ISemantic semantic)
        {
            if (!(semantic is IRef r))
                return null;

            // Type of Ref = Type / Variable / Property / Field / 
            // var def = 
        }
        */

        public IStatement Create(ISemantic parent, PlatoStatement statement)
        {
            var r = Create<Statement>(parent, statement) as IStatement;

            if (statement is PlatoVarDeclStatement pvds)
            {
                r = Create<VarDeclaration>(parent, pvds) as IStatement;
            }

            foreach (var childStatement in statement.ChildStatements)
            {
                Create(r, childStatement);
            }

            foreach (var childExpression in statement.ChildExpressions)
            {
                Create(r, childExpression);
            }

            return r; 
        }

        public ILambda Create(ISemantic parent, PlatoLambda lambda)
        {
            var r = Create<Lambda>(parent, lambda);
            var f = Create<FunctionDef>(r, lambda);
            f.Body = Create(r, lambda.Body);
            foreach (var p in  lambda.Parameters.Parameters)
            {
                f.Parameters.Add(Create(f, p));
            }
            r.Function = f;
            return r;
        }

        public IVarRef Create(ISemantic parent, PlatoIdentifierRef identifier)
        {
            var r = Create<VarRef>(parent, identifier);
            r.Type = Create(parent, identifier.Type);
            return r;
        }

        public IExpression Create(ISemantic parent, PlatoExpression expression)
        {
            // TODO: check for Assignement 
            
            switch (expression)
            {
                case PlatoLambda platoLambda:
                    return Create(parent, platoLambda);

                case PlatoIdentifierRef platoIdentifierRef:
                    return Create(parent, platoIdentifierRef);

                case PlatoArg platoArg:
                    break;
                case PlatoArray platoArray:
                    break;
                case PlatoAssignment platoAssignment:
                    break;
                case PlatoBinary platoBinary:
                    break;
                case PlatoCast platoCast:
                    break;
                case PlatoConditional platoConditional:
                    break;
                case PlatoDefault platoDefault:
                    break;
                case PlatoElementGet platoElementGet:
                    break;
                case PlatoElementSet platoElementSet:
                    break;
                case PlatoInterpolation platoInterpolation:
                    break;
                case PlatoInvoke platoInvoke:
                    break;
                case PlatoNameOf platoNameOf:
                    break;
                case PlatoTypeOf platoTypeOf:
                    break;
                case PlatoLiteral platoLiteral:
                    break;
                case PlatoMemberGet platoMemberGet:
                    break;
                case PlatoMemberSet platoMemberSet:
                    break;
                case PlatoNew platoNew:
                    break;
                case PlatoParenthesis platoParenthesis:
                    break;
                case PlatoPostfix platoPostfix:
                    break;
                case PlatoPrefix platoPrefix:
                    break;
                case PlatoThis platoThis:
                    break;
                case PlatoThrowExpression platoThrowExpression:
                    break;
                case PlatoTuple platoTuple:
                    break;
                case PlatoTypeExpr platoTypeExpr:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }

            var r = Create<Expression>(parent, expression);

            foreach (var childExpression in expression.ChildExpressions)
            {
                Create(r, childExpression);
            }

            return r;
        }

        public IClass Create(ISemantic parent, PlatoClass c)
        {
            var r = Create<Class>(parent, c);

            r.Name = c.Name;

            foreach (var t in c.TypeParameters.TypeParameters)
                r.TypeParams.Add(Create(r, t));
            
            foreach (var p in c.Properties)
                Create<Property>(r, p);
            
            foreach (var f in c.Fields)
                Create<Field>(r, f);


            foreach (var m in c.Methods)
            {
                var method = Create<Method>(r, m);
                method.Function = Create(method, m);
            }

            return r;
        }

        public void CreateDefs()
        {
            foreach (var c in Mapping.GetClasses())
            {
                Create(null, c);
            }
        }

        public void CreateRefs()
        {

        }
    }
}
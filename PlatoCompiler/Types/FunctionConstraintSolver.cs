using System.Collections.Generic;
using Ara3D.Utils;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    /*
     * Let's solve the first problem ...  
     */
  
    public class ConstraintSolver
    {
        public HashSet<TypeVariableConstraint> Constraints { get; }

        public IReadOnlyList<TypeExpression> Arguments { get; }
        public IReadOnlyList<TypeExpression> Parameters { get; }

        public bool IsSatisfied { get; set; }
        public TypeExpression FinalResult { get; set; }
        public string Message { get; set; }

        public ConstraintSolver(
            IReadOnlyList<TypeExpression> arguments,
            IReadOnlyList<TypeExpression> parameters,
            TypeExpression result)
        {
            Arguments = arguments;
            Parameters = parameters;
        }

        public void LogError(string message)
        {
            Message = message;
        }

        public void AddDeclaredConstraint(TypeParameterDefinition tpd)
        {
            var constraint = tpd.Constraint;
            if (constraint == null)
                return;
            // TODO: add the constraint to the set of constraints. 
        }

        public void AddEqualsConstraint(TypeParameterDefinition tpd, TypeExpression tes)
        {
            if (tpd.Equals(tes))
                return;

            // TODO: add the constraint to the set of constraints 
        }

        public void AddImplementsConstraint(TypeExpression tes, TypeDefinition tds)
        {
            Verifier.Assert(tds.IsConcept());

            // If they are equals, no work to do. 
            if (tds.Equals(tes.Definition))
                return;

            if (tes.Definition.IsConcept())
            {
                if (!tes.Definition.InheritsFrom(tds))
                {
                    LogError($"Type expression {tes} does not implement {tds}");
                }

                // It inherits from the other. 
                // However, are there potential additional type variables to constraint? 
            }
        }

        public void AddCastConstraint(TypeExpression a, TypeDefinition b)
        {

        }

        public void GatherConstraints(TypeExpression a, TypeExpression b)
        {
            if (a.Equals(b))
                return;

            Verifier.Assert(!a.Definition.IsLibrary());
            Verifier.Assert(!b.Definition.IsLibrary());

            if (a.Definition is TypeParameterDefinition tpd)
            {
                Verifier.Assert(a.TypeArgs.Count == 0);
                AddDeclaredConstraint(tpd);
                AddEqualsConstraint(tpd, b);
            }

            if (b.Definition is TypeParameterDefinition tpdOther)
            {
                Verifier.Assert(b.TypeArgs.Count == 0);
                AddDeclaredConstraint(tpdOther);
                AddEqualsConstraint(tpdOther, b);
            }

            if (a.Definition.IsConcept())
            {
                AddImplementsConstraint(b, a.Definition);
            }

            if (b.Definition.IsConcept())
            {
                AddImplementsConstraint(a, b.Definition);
            }

            if (b.Definition.IsConcrete())
            {
                AddCastConstraint(a, b.Definition);
            }
        }
    }
}
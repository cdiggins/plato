using System;
using System.Collections.Generic;

namespace Plato.Compiler.Types
{
    public class ConstraintTree
    {
        public string Message { get; private set; }
        public bool Validated { get; private set; } = false;
        public bool ValidationResult { get; private set; } = false;

        public void SetValidity(bool validationResult, string reason = "")
        {
            ValidationResult = validationResult;
            Message = reason;
            Validated = true;
        }

        public bool IsValid()
        {
            if (!Validated) throw new InvalidOperationException("Validation has not occurred");
            return ValidationResult;
        }

        public List<IConstraint> Constraints { get; } = new List<IConstraint>();
        public List<ConstraintTree> Children { get; } = new List<ConstraintTree>();
    }
}
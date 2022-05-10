using System;
using System.Collections.Generic;
using System.Text;

namespace PlatoIR
{
    public class IROptimizer
    {
        public bool Optimize(IR ir)
        {
            ClassToStruct(ir);
            return true;
        }

        public IR ClassToStruct(IR ir)
            => ir is TypeDeclarationIR typeDeclaration && typeDeclaration.Kind == "class" && !typeDeclaration.IsStatic
                ? typeDeclaration.SetKind("readonly struct")
                : ir;
    }
}

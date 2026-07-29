// Anti-vacuous-green guard for the forward conformance suite.
//
// The forward stdlib does not yet codegen (see README.md), so Generated\ may be empty.
// An empty Generated\ makes LawTests.LawCases() yield ZERO cases -- which NUnit would
// otherwise report as a passing (vacuous) run. That would hide the fact that nothing is
// actually being tested. This guard turns "no forward law types were generated" into an
// explicit FAILURE, so the suite is RED until real codegen lands, never falsely green.
using System.Reflection;

namespace Plato.ForwardConformanceTests
{
    [TestFixture]
    public class BlockerGuardTests
    {
        [Test]
        public void ForwardLawTypesWereGenerated()
        {
            var caseCount = LawTests.LawCases().Count();
            Assert.That(caseCount, Is.GreaterThan(0),
                "No forward Law_* cases were discovered in the generated assembly. "
                + "This means Generated\\ is empty or carries no Law_ members -- forward-stdlib "
                + "codegen is still BLOCKED (Plato.CSharpWriter: 'No ground TIR for bodied ...'). "
                + "Run tools\\regen-forward-conformance.ps1 -Codegen to see the first blocker. "
                + "Remove/relax this guard only once codegen produces the law types.");
        }

        [Test]
        public void GeneratedTypesArePresent()
        {
            var generatedTypes = typeof(BlockerGuardTests).Assembly.GetTypes()
                .Count(t => t.Namespace == "Ara3D.Geometry" && t.IsPublic);
            Assert.That(generatedTypes, Is.GreaterThan(0),
                "No Ara3D.Geometry types found in the assembly: forward-stdlib codegen did not "
                + "produce any .g.cs (see tools\\regen-forward-conformance.ps1).");
        }
    }
}

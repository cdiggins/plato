# Verification projects

Each project tests the production component named in its directory. [PlatoTests](PlatoTests/README.md) covers compiler and writer contracts; [interpreter tests](Plato.Interpreter.Tests/README.md) cover execution and laws; [navigation tests](Plato.Navigation.Tests/README.md) cover source queries. Fixture directories beginning `plato-test-` hold input programs, while `conformance/` and `optimizer-smoke/` hold generated-program harnesses. Run existing project tests after the relevant source builds; gate policy lives in [stdlib/VERIFICATION.md](../stdlib/VERIFICATION.md).

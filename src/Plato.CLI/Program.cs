// Routes command-line invocations to their command implementations.
using Ara3D.Logging;

namespace Ara3D.Geometry.CLI;

public static class Program
{
    public static int Main(string[] args)
    {
        var logger = Logger.Console;
        return args.FirstOrDefault() switch
        {
            "lint" => Lint(args.Skip(1).ToArray(), logger),
            "run" => RunCommand.Run(args.Skip(1).ToArray(), logger),
            _ => GenerateCommand.Run(args, logger)
        };
    }

    public static int Lint(string[] args, ILogger logger)
        => LintCommand.Run(args, logger);
}

using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;

try
{
    var options = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var i = 0; i < args.Length; i += 2)
    {
        if (i + 1 >= args.Length || args[i] is not ("--worker" or "--fixture")
            || string.IsNullOrWhiteSpace(args[i + 1]) || !options.TryAdd(args[i], args[i + 1]))
            throw new ArgumentException("Expected distinct --worker <worker.dll> and --fixture <fixture.json> arguments");
    }
    if (options.Count != 2)
        throw new ArgumentException("Both --worker and --fixture are required");
    var workerPath = Path.GetFullPath(options["--worker"]);
    var fixturePath = Path.GetFullPath(options["--fixture"]);
    var workerFolder = Path.GetDirectoryName(workerPath)!;
    var workerBytes = File.ReadAllBytes(workerPath);
    var fixtureBytes = File.ReadAllBytes(fixturePath);
    AssemblyLoadContext.Default.Resolving += (context, name) =>
    {
        var simpleName = name.Name;
        if (string.IsNullOrWhiteSpace(simpleName) || simpleName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || simpleName.Contains('/') || simpleName.Contains('\\') || Path.GetFileName(simpleName) != simpleName)
            throw new FileLoadException($"Unsafe dependency assembly name: {simpleName}");
        var dependencyPath = Path.GetFullPath(Path.Combine(workerFolder, simpleName + ".dll"));
        if (!string.Equals(Path.GetDirectoryName(dependencyPath), workerFolder,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new FileLoadException($"Dependency leaves worker folder: {simpleName}");
        return File.Exists(dependencyPath) ? context.LoadFromAssemblyPath(dependencyPath) : null;
    };
    var worker = AssemblyLoadContext.Default.LoadFromAssemblyPath(workerPath);
    var fixtureType = worker.GetType("Plato.OptimizerBenchmark.PreparedFixture", throwOnError: true)!;
    var read = fixtureType.GetMethod("Read", BindingFlags.Public | BindingFlags.Static, null, [typeof(string)], null)
        ?? throw new MissingMethodException(fixtureType.FullName, "Read(string)");
    var fixture = read.Invoke(null, [fixturePath])
        ?? throw new InvalidDataException("Worker returned an empty prepared fixture");
    var jsonType = worker.GetType("Plato.OptimizerBenchmark.PortableJson", throwOnError: true)!;
    var serialize = jsonType.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(m => m.Name == "Bytes" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1
            && m.GetParameters().Length == 1)
        .MakeGenericMethod(fixtureType);
    var restoredBytes = serialize.Invoke(null, [fixture]) as byte[]
        ?? throw new InvalidDataException("Worker serializer did not return fixture bytes");
    if (!fixtureBytes.AsSpan().SequenceEqual(restoredBytes))
        throw new InvalidDataException("Fixture does not round-trip byte for byte through this worker");
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        schema = 1, status = "ok",
        fixture_hash = Convert.ToHexString(SHA256.HashData(fixtureBytes)).ToLowerInvariant(),
        worker_hash = Convert.ToHexString(SHA256.HashData(workerBytes)).ToLowerInvariant(),
    }));
    return 0;
}
catch (Exception exception)
{
    var error = exception;
    while (error is TargetInvocationException { InnerException: not null } invocation)
        error = invocation.InnerException!;
    Console.WriteLine(JsonSerializer.Serialize(new { schema = 1, status = "error", error = error.ToString() }));
    return 1;
}

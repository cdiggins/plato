using System.Globalization;
using System.Text.Json;
using Plato.OptimizerBenchmark;

try
{
    if (args.Length == 0) throw new ArgumentException("Expected prepare, run, run-set, or list");
    var options = new Dictionary<string, string>();
    for (var i = 1; i < args.Length; i += 2)
    {
        if (!args[i].StartsWith("--", StringComparison.Ordinal) || i + 1 >= args.Length)
            throw new ArgumentException($"Expected --option value at {args[i]}");
        if (!options.TryAdd(args[i][2..], args[i + 1]))
            throw new ArgumentException($"Repeated option {args[i]}");
    }
    string Required(string name) => options.TryGetValue(name, out var value) ? value : throw new ArgumentException($"Missing --{name}");
    double Number(string name, string fallback) => double.Parse(options.GetValueOrDefault(name, fallback), CultureInfo.InvariantCulture);
    int Integer(string name, string fallback) => int.Parse(options.GetValueOrDefault(name, fallback), CultureInfo.InvariantCulture);
    object result;
    switch (args[0])
    {
        case "list":
            result = new { schema = 1, witnesses = PreparedFixture.Witnesses(), workloads = Operations.Names };
            break;
        case "prepare":
            var chordal = Number("chordal", "0.01");
            if (!double.IsFinite(chordal) || chordal <= 0) throw new ArgumentOutOfRangeException("chordal");
            var fixture = PreparedFixture.Create(Required("input"), Required("id"), chordal);
            var bytes = PortableJson.Bytes(fixture);
            var path = Path.GetFullPath(Required("output"));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
            var restored = PreparedFixture.Read(path);
            if (!bytes.AsSpan().SequenceEqual(PortableJson.Bytes(restored)))
                throw new InvalidDataException("Prepared fixture did not round-trip exactly");
            result = new { schema = 1, id = fixture.Id, fixture = path, fixture_hash = PortableJson.Hash(bytes),
                tags = fixture.Tags, workloads = fixture.Workloads, input_hash = fixture.InputHash,
                preparation_identity = fixture.PreparationIdentity, chordal = fixture.Chordal,
                edges = fixture.Cached.Edges.Count, faces = fixture.Cached.Faces.Count };
            break;
        case "run":
            result = Measurement.Run(PreparedFixture.Read(Required("fixture")), Required("workload"),
                Integer("warmup", "2"), Number("target-ms", "100"), Integer("max-iterations", "100000"), Number("warmup-ms", "200"));
            break;
        case "run-set":
            var prepared = PreparedFixture.Read(Required("fixture"));
            var workloads = Required("workloads").Split(',');
            if (workloads.Any(string.IsNullOrWhiteSpace) || workloads.Distinct().Count() != workloads.Length)
                throw new ArgumentException("Workload set must contain distinct nonempty names");
            var samples = new List<object>();
            foreach (var workload in workloads)
            {
                try
                {
                    samples.Add(Measurement.Run(prepared, workload, Integer("warmup", "2"),
                        Number("target-ms", "100"), Integer("max-iterations", "100000"), Number("warmup-ms", "200")));
                }
                catch (Exception exception)
                {
                    samples.Add(new { schema = 1, status = "error", case_id = prepared.Id, workload, error = exception.ToString() });
                }
            }
            result = new { schema = 1, samples };
            break;
        default:
            throw new ArgumentException($"Unknown command {args[0]}");
    }
    Console.WriteLine(JsonSerializer.Serialize(result, PortableJson.Options));
    return 0;
}
catch (Exception exception)
{
    Console.WriteLine(JsonSerializer.Serialize(new { schema = 1, status = "error", error = exception.ToString() }, PortableJson.Options));
    return 1;
}

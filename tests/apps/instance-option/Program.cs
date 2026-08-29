using SnapCLI;

// Fixture for SampleTests: declares [Option] on a non-static property. SnapCLI must report this
// as an attribute usage error rather than failing later when the default value is evaluated.

class Program
{
    [Option(Name = "instance-opt", Description = "declared on an instance property")]
    public string InstanceOpt { get; set; } = "x";

    [RootCommand(Description = "instance option fixture")]
    public static void Root() => Console.WriteLine("should never run");
}

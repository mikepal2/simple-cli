using SnapCLI;

// Fixture for SampleTests: declares a top level command with the same name as the executable,
// which System.CommandLine cannot represent. SnapCLI must report this as an attribute usage
// error rather than failing later inside the tokenizer.

[assembly: Command(Name = "shadow-name", Description = "shadows the executable name")]

class Program
{
    [Command(Name = "shadow-name sub")]
    public static void Sub() => Console.WriteLine("should never run");
}

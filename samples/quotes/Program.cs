//
// This program is based on the sample from the System.CommandLine documentation:
// https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial#add-subcommands-and-custom-validation
// (scroll down to "The finished app looks like this:" section)
//
// It is functionally equivalent to the original code but adapted to use the SnapCLI library, 
// demonstrating how it simplifies development, readability, and maintenance of the code.
//

using SnapCLI;

// these commands have no associated handler methods, and therefore declared at assembly level
[assembly: RootCommand(Description = "Sample app for SnapCLI")]
[assembly: Command(Name = "quotes", Description = "Work with a file that contains quotes.")]

class Program
{
    // global option with validation
    // CommandLineInputException marks this as bad user input, so the default exception handler
    // reports it as a short error message instead of a stack trace.
    [Option(Name = "file", Description = "An option whose argument is parsed as a FileInfo")]
    public static FileInfo file  {
        get { return _file; }
        set {
            if (!value.Exists)
                throw new CommandLineInputException($"File not found: {value.FullName}");
            _file = value;
        }
    }
    private static FileInfo _file = new FileInfo("sampleQuotes.txt");

    [Command(Name = "quotes read", Description = "Read and display the file.")]
    public static async Task ReadFile(
        [Option(Description = "Delay between lines, specified as milliseconds per character in a line.")]   
        int delay = 42,
        
        [Option(Name = "fgcolor", Description = "Foreground color of text displayed on the console.")]
        ConsoleColor fgColor = ConsoleColor.White,
        
        [Option(Description = "Background color of text displayed on the console: default is black, light mode is white.")]
        bool lightMode = false)
    {
        Console.BackgroundColor = lightMode ? ConsoleColor.White : ConsoleColor.Black;
        Console.ForegroundColor = fgColor;
        var lines = File.ReadLines(file.FullName).ToList();
        foreach (string line in lines)
        {
            Console.WriteLine(line);
            await Task.Delay(delay * line.Length);
        };

    }

    [Command(Name = "quotes delete", Description = "Delete lines from the file.")]
    internal static void DeleteFromFile(
        [Option(Description = "Strings to search for when deleting entries.")] string[] searchTerms
        )
    {
        Console.WriteLine("Deleting from file");
        File.WriteAllLines(
            file.FullName, File.ReadLines(file.FullName)
                .Where(line => searchTerms.All(s => !line.Contains(s))).ToList());
    }

    [Command(Name = "quotes add", Description = "Add an entry to the file.", Aliases = "insert")]
    internal static void AddToFile(
        [Argument(Description = "Text of quote.")]   string quote,
        [Argument(Description = "Byline of quote.")] string byline
        )
    {
        Console.WriteLine("Adding to file");
        using StreamWriter? writer = file.AppendText();
        writer.WriteLine($"{Environment.NewLine}{Environment.NewLine}{quote}");
        writer.WriteLine($"{Environment.NewLine}-{byline}");
        writer.Flush();
    }
}

using SnapCLI;

// This CLI example uses classic Main() behavior.
// See documentation for details: https://github.com/mikepal2/snap-cli/blob/main/docs/Documentation.md#main-method

class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Your initialization code here
        // ...

        return await SnapCLI.CLI.RunAsync(args);
    }

    [RootCommand]
    static void RootCommandHandler(
        [Argument] string name = "World",
        ConsoleColor fgColor = ConsoleColor.White,
        int repeat = 1)
    {
        Console.ForegroundColor = fgColor;
        for (int i = 0; i < repeat; i++)
            Console.WriteLine($"Hello {name}!");
    }
}

/* This program generates following output
   
> classic-main -?
Description:

Usage:
  classic-main [<name>] [options]

Arguments:
  <name>  [default: World]

Options:
  --fg-color <Black|Blue|Cyan|DarkBlue|DarkCyan|DarkGray|DarkGreen|DarkMagenta|DarkRed|DarkYellow|Gray|Green|Magenta|Red|White|Yellow>  [default: White]
  --repeat <repeat>                                                                                                                     [default: 1]
  -?, -h, --help                                                                                                                        Show help and usage information
  --version                                                                                                                             Show version information

> classic-main Joe --fg-color Yellow --repeat 2
Hello Joe!
Hello Joe!

 */



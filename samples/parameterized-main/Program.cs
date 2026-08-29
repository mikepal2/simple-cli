using SnapCLI;

// Simple CLI example with parameterized Main() method.
//
// Since there are no methods declared with the [Command] attribute, the SnapCLI library automatically binds the Main() method as the root command handler.
// Any parameters of the Main() method are bound as options by default, even if they are not explicitly declared with the [Option] attribute,
// while [Argument] attribute is used to declare a parameter to be bound as an argument. 
//
// This default behavior allows you to write simple CLI applications with minimal effort. See the documentation for more details.

class Program
{
    static void Main(
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
   
> parameterized-main -?
Description:

Usage:
  parameterized-main [<name>] [options]

Arguments:
  <name>  [default: World]

Options:
  --fg-color <Black|Blue|Cyan|DarkBlue|DarkCyan|DarkGray|DarkGreen|DarkMagenta|DarkRed|DarkYellow|Gray|Green|Magenta|Red|White|Yellow>  [default: White]
  --repeat <repeat>                                                                                                                     [default: 1]
  -?, -h, --help                                                                                                                        Show help and usage information
  --version                                                                                                                             Show version information

> parameterized-main Joe --fg-color Yellow --repeat 2
Hello Joe!
Hello Joe!

 */



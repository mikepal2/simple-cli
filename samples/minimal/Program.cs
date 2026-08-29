using SnapCLI;

// Minimal CLI example

internal class Program
{
    // By using the [RootCommand] attribute, we designate the Hello() method as the root command handler. This means the method is executed by default,
    // and no command name needs to be specified on the command line. Additionally, any parameters of the method are treated as options by default, even
    // if they are not explicitly declared with the [Option] attribute.
    [RootCommand]
    public static void Hello(string name = "World")
    {
        Console.WriteLine($"Hello {name}!");
    }
}

/* This program will produce output:

> minimal.exe --help
Description:

Usage:
  minimal [options]

Options:
  --name <name>   [default: World]
  -?, -h, --help  Show help and usage information
  --version       Show version information


> minimal.exe
Hello World!

> minimal.exe --name Joe
Hello Joe!
  
*/
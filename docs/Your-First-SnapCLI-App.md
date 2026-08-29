# Building your first app with SnapCLI

This walkthrough will show you how to get started using the SnapCLI app model to build a command line application.

# Simple app

Let's say your CLI application is pretty simple - it is performing single task and only need a few options to extend or control its functionality. In this case the application code is usually placed directly in `Main` method.

## Create a new console app

Open a new console, go to your projects directory and run the following commands:

```console
> dotnet new console -o myApp --use-program-main
> cd myApp
```

You can build and run it with following commands:

```console
> dotnet build
> ./bin/Debug/myApp [options]
Hello World!
```

or run with dotnet:

```console
> dotnet run -- [options]
Hello World!
```

For this tutorial, when showing examples of command lines for the program, we will assume that the program is rebuilt and the binary is running directly from its folder, so the command will look like this:

```console
> myApp [options]
Hello World!
```

## Install the SnapCLI package

To use SnapCLI library you need to add nuget package to the project.

[![Nuget](https://img.shields.io/nuget/v/SnapCLI.svg)](https://nuget.org/packages/SnapCLI)

You can add it using IDE or from command line using command `dotnet add package SnapCLI`.

## Add some code

Open `Program.cs`. You'll see that your `Main` method looks like this:

```csharp
static void Main(string[] args)
{
    Console.WriteLine("Hello World!");
}
```

This is default main and it only takes `string` array as parameter. With SnapCLI, you can accept named parameters of various types and specify default values. Let's change your `Main` method to this:

```csharp
using SnapCLI;

class Program
{
    static void Main(
        string name = "World", 
        ConsoleColor fgColor = ConsoleColor.White, 
        int repeat = 1)
    {
        Console.ForegroundColor = fgColor;
        for (int i=0; i<repeat; i++)
            Console.WriteLine($"Hello {name}!");
    }
}
```

> **Technical note:** In this example, the SnapCLI library automatically recognizes the `Main` method as the [root command](./Documentation.md#root-command) handler because the program has no methods declared with `[RootCommand]` or `[Command]` attributes. All the parameters of the `Main` method are bound to options with the same names. This default behavior allows you to write simple CLI applications with minimal effort.

You're ready to run your program.

 <p style="font-family:SFMono-Regular, Menlo, Monaco, Consolas, liberation mono, courier new, monospace; background-color:black;">
   <span style="color:lightgray">> myApp</span><br>
   <span style="color:white">Hello World!</span><br>
   <br>
   <span style="color:lightgray">> myApp --name Michael --fg-color Yellow --repeat 3</span><br>
   <span style="color:yellow">Hello Michael!</span><br>
   <span style="color:yellow">Hello Michael!</span><br>
   <span style="color:yellow">Hello Michael!</span><br>
 </p>


By default, SnapCLI binds every parameter of `Main` to an option. You can also have positional arguments by using the `[Argument]` attribute before a parameter. Let's change `name` to be an argument in your code:

```csharp
using SnapCLI;

class Program
{
    static void Main(
        [Argument] string name = "World", 
        ConsoleColor fgColor = ConsoleColor.White, 
        int repeat = 1)
    {
        Console.ForegroundColor = fgColor;
        for (int i=0; i<repeat; i++)
            Console.WriteLine($"Hello {name}!");
    }    
}
```

Now you don't need to specify `--name` when providing name argument on the command line.

 <p style="font-family:SFMono-Regular, Menlo, Monaco, Consolas, liberation mono, courier new, monospace; background-color:black;">
   <span style="color:lightgray">> myApp</span><br>
   <span style="color:white">Hello World!</span><br>
   <br>
   <span style="color:lightgray">> myApp Michael</span><br>
   <span style="color:white">Hello Michael!</span><br>
   <br>
   <span style="color:lightgray">> myApp Michael --fg-color green</span><br>
   <span style="color:green">Hello Michael!</span><br>
 </p>

## App Help

Your program already has basic help listing all arguments and options!

```text
> myApp -?
Description:

Usage:
  myApp [<name>] [options]

Arguments:
  <name>  [default: World]

Options:
  --fg-color <Black|Blue|Cyan|DarkBlue|DarkCyan|DarkGray|DarkGreen|DarkMagenta|DarkRed|DarkYellow|Gray|Green|Magenta|Red|White|Yellow>  
                           [default: White]
  --repeat <repeat>        [default: 1]
  -?, -h, --help           Show help and usage information
  --version                Show version information
```

You can enhance it by providing descriptions. 

```csharp
using SnapCLI;

class Program
{
    [RootCommand(Description = "Our sample Hello application")]
    static void Main(
        [Argument(Description = "The name to greet")]
        string name = "World",

        [Option(Description = "Foreground color for console output")]
        ConsoleColor fgColor = ConsoleColor.White,

        [Option(Description = "Number of lines to output")]
        int repeat = 1
    )
    {
        Console.ForegroundColor = fgColor;
        for (int i=0; i<repeat; i++)
            Console.WriteLine($"Hello {name}!");
    }
}
```

```text
> myApp -?
Description:
  Our sample Hello application

Usage:
  myApp [<name>] [options]

Arguments:
  <name>  The name to greet [default: World]

Options:
  --fg-color <Black|Blue|Cyan|DarkBlue|DarkCyan|DarkGray|DarkGreen|DarkMagenta|DarkRed|DarkYellow|Gray|Green|Magenta|Red|White|Yellow>  
                            Foreground color for console output [default: White]
  --repeat <repeat>         Number of lines to output [default: 1]
  -?, -h, --help            Show help and usage information
  --version                 Show version information
```


# Documentation

For Quick Start guide and more details on SnapCLI library features see [documentation](Documentation.md).


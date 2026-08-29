# Building Multi-Command CLI

Let's say you need an app that will have multiple commands.

In this walkthrough, we will create an app to encode and decode base64 format using the SnapCLI app model.

## Create Project

```console
> dotnet new console -o base64
> cd base64
> dotnet add package SnapCLI
```

## Create Commands

Replace the content of `Program.cs` with the following code.

Here, we define two commands — `encode` and `decode` — both accepting a string argument and printing the result to the console.

```csharp
using SnapCLI;

class Program
{
    [Command]
    public static void Encode([Argument] string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        var base64 = System.Convert.ToBase64String(bytes);
        Console.WriteLine(base64);
    }

    [Command]
    public static void Decode([Argument] string base64)
    {
        var bytes = System.Convert.FromBase64String(base64);
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        Console.WriteLine(text);
    }
}
```

> **Technical note:** In this example, the two methods `Encode()` and `Decode()` are declared with the `[Command]` attribute and represent two entry points for the CLI application, each associated with the corresponding command. The command names are automatically derived from the method names as `encode` and `decode`, respectively.

> Also note that the program does not have a `Main` method, as the SnapCLI library takes responsibility for starting up the application. See more details on `Main` in the [documentation](Documentation.md#main-method).

The application is ready to run.

```console
> base64 -?
Description:

Usage:
  base64 [command] [options]

Options:
  -?, -h, --help  Show help and usage information
  --version       Show version information

Commands:
  encode <text>
  decode <base64>

> base64 encode "Hello World!"
SGVsbG8gV29ybGQh

> base64 decode SGVsbG8gV29ybGQh
Hello World!
```

Now, let's add descriptions to make the help more user-friendly.

```csharp
using SnapCLI;

[assembly: RootCommand(Description = "Base64 encoder/decoder")]

class Program
{
    [Command(Description = "Encode text to base64")]
    public static void Encode(
        [Argument(Description = "Text to encode")] string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        Console.WriteLine(System.Convert.ToBase64String(bytes));
    }

    [Command(Description = "Decode text from base64")]
    public static void Decode(
        [Argument(Description = "Base64 string to decode")] string base64)
    {
        var bytes = System.Convert.FromBase64String(base64);
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(bytes));
    }
}
```

The help output now describes the program and both commands:

```console
> base64 -?
Description:
  Base64 encoder/decoder

Usage:
  base64 [command] [options]

Options:
  -?, -h, --help  Show help and usage information
  --version       Show version information

Commands:
  encode <text>    Encode text to base64
  decode <base64>  Decode text from base64
```

Now, let's make it a bit more complex by adding the ability to read input from a file and write output to a file. For this, we will add `--input` and `--output` options for each command. We will also declare that the string argument and the `--input` option are mutually exclusive, i.e., only one of them can be specified on the command line at a time.

```csharp
using SnapCLI;

[assembly: RootCommand(Description = "Base64 encoder/decoder")]

class Program
{
    [Command(Description = "Encode text to base64",
             MutuallyExclusiveOptionsArguments = "(text,input)")]
    public static void Encode(
        [Argument(Description = "Text to encode")] string? text = null,
        [Option(Description = "Read input from file")] FileInfo? input = null,
        [Option(Description = "Write output to file")] FileInfo? output = null)
    {
        var bytes = input != null
            ? File.ReadAllBytes(input.FullName)
            : System.Text.Encoding.UTF8.GetBytes(text ?? throw new CommandLineInputException("Either <text> or --input is required"));

        Write(output, System.Convert.ToBase64String(bytes));
    }

    [Command(Description = "Decode text from base64",
             MutuallyExclusiveOptionsArguments = "(base64,input)")]
    public static void Decode(
        [Argument(Description = "Base64 string to decode")] string? base64 = null,
        [Option(Description = "Read input from file")] FileInfo? input = null,
        [Option(Description = "Write output to file")] FileInfo? output = null)
    {
        var text = input != null
            ? File.ReadAllText(input.FullName)
            : base64 ?? throw new CommandLineInputException("Either <base64> or --input is required");

        Write(output, System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(text)));
    }

    private static void Write(FileInfo? output, string content)
    {
        if (output != null)
            File.WriteAllText(output.FullName, content);
        else
            Console.WriteLine(content);
    }
}
```

Specifying both the argument and `--input` is now rejected with a short error message:

```console
> base64 encode "Hello World!" --input data.txt
Error: argument 'text' and option '--input' are mutually exclusive for command 'encode'
```

See the [documentation](Documentation.md#mutually-exclusive-options-and-arguments) for more on
declaring mutually exclusive options and arguments.


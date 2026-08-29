# SnapCLI Library

- [Overview](#overview)
- [Command Line Syntax](#command-line-syntax)
- [Quick Start](#quick-start)
  - [Commands](#commands)
  - [Options](#options)
  - [Arguments](#arguments)
- [Advanced Usage](#advanced-usage)
  - [Root Command](#root-command)
  - [Subcommands](#subcommands)
  - [Commands without Handlers](#commands-without-handlers)
  - [Recursive Options](#recursive-options)
  - [Global Options](#global-options)
  - [Arity](#arity)
  - [Kebab Case](#kebab-case)
- [Application Lifecycle](#application-lifecycle)
  - [Main Method](#main-method)
  - [Startup](#startup)
  - [BeforeCommand](#beforecommand)
  - [AfterCommand](#aftercommand)
- [Validation](#validation)
  - [Input Validation](#input-validation)
  - [Mutually Exclusive Options and Arguments](#mutually-exclusive-options-and-arguments)
- [Exception Handling](#exception-handling)

> **Note**: Links to external sources and Microsoft documentation will be included throughout the text below, marked with the 🗗 symbol.

## Overview

This library enables developers to quickly create POSIX-like CLI applications by automatically managing command-line commands and parameters using the provided metadata. This simplifies the development process and allows developers to focus on their application logic. Additionally, it streamlines the creation of the application's help system, ensuring that all necessary information is easily accessible to end users.

The library employs an API paradigm that utilizes [attributes🗗](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/reflection-and-attributes/) to declare and describe CLI commands, options, and arguments through metadata.

Any static method can be designated as a CLI command handler using the `[Command]` attribute (it does not need to be public), which serves as the entry point for that command within the CLI application. Each parameter of the command handler method automatically becomes a command option. Refer to the sections below for further details and examples.

There’s even [no need](#main-method) to write a `Main()` method for the application, allowing developers to skip any startup boilerplate code and dive straight into implementing the application logic, i.e., commands.

While many CLI frameworks require separate class implementations for each command, this project adopts an approach that minimizes the use of classes. Creating individual classes for each command can introduce unnecessary complexity with minimal benefit. Instead, it relies on attributes to describe entities. By using attributes, we simplify maintenance and enhance readability, as they are declared close to the entities they describe, keeping related information in one place. Attributes can also provide additional details, such as descriptions and aliases.

Although this approach may not offer the same flexibility as some alternatives, it effectively meets the needs of most CLI applications, and [additional customizations](#advanced-usage) are also possible.

## Command Line Syntax

Since this project is based on the [System.CommandLine🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) library, the parsing rules align with those established by that package. Microsoft provides detailed explanations of the [command-line syntax🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax) recognized by `System.CommandLine`.

It is recommended to follow the [System.CommandLine design guidance🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/design-guidance) when designing your CLI.

## Quick Start

Here, we will explore basic constructs to define [commands](#commands), [options](#options) and [arguments](#arguments). For many CLI application that is all they need to get and process command line input.

### Commands

A [command🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#commands) in command-line input is a token that specifies an action or defines a group of related actions. Sometimes commands may be referred as *verbs*.

Any static method can be declared as a CLI command handler using the `[Command]` attribute. The method does not have to be public - `internal` and `private` handlers work too. In its minimal form, it can be used as follows:

```csharp
using SnapCLI;
class Program
{
    [Command]
    public static void Hello() 
    {
        Console.WriteLine("Hello World!");
    }
}
```

Additional information can be provided in attribute parameters to enhance command-line parsing and the help system, such as the command's explicit name, aliases, description, and whether the command is hidden.

```csharp
using SnapCLI;
class Program
{
    [Command(Name = "hello", Aliases = "hi,hola,bonjour", Description = "Hello example", Hidden = false)]
    public static void Hello() 
    {
        Console.WriteLine("Hello World!");
    }
}
```

Async handler methods are also supported.

The library supports handler methods with the following return types: `void`, `int`, `Task<int>`, `Task`, `ValueTask<int>`, and `ValueTask`. The result from handlers returning `int`, `Task<int>`, and `ValueTask<int>` is used as the program's exit code.

```csharp
using SnapCLI;
class Program
{
  [Command(Name = "sleep", Description = "Async sleep example")]
  public static async Task<int> Sleep(int milliseconds = 1000)
  {
      Console.WriteLine("Sleeping...");
      await Task.Delay(milliseconds);
      Console.WriteLine("OK");
      return 0; // exit code
  }
}
```

**Command name convention**

- If the `[Command]` attribute does not specify a command name:
  - If this is the only command in the program, it is automatically treated as the [root command](#root-command).
  - If there are multiple commands declared, the method name, converted to [kebab case](#kebab-case), is used as the command name. For example, the method `Hello()` will handle the `hello` command, while method `HelloWorld()` will handle `hello-world` command.
  - If the method name contains underscores (`_`), it declares a [subcommand](#subcommands). For example, a method named "order_create()" will define a subcommand `create` under the `order` command.
- If the name specified in the `[Command]` attribute explicitly contains spaces, it declares a [subcommand](#subcommands). For example, `[Command(Name = "order create")]` defines `create` as a subcommand of the `order` command.
- Commands may have [aliases🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#aliases). These are usually short forms that are easier to type or alternate spellings of a word.
- Command names and aliases are [case-sensitive🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#case-sensitivity). If you want your CLI to be case insensitive, define aliases for the various casing alternatives.

### Options

An [option🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#options) is a named parameter that can be passed to a command.

Any parameter of command handler method automatically becomes a command option. In the next example `name` becomes option for command `hello`:

```csharp
[Command(Name = "hello", Description = "Hello example", Hidden = false)]
public static void Hello(string name = "World") 
{
    Console.WriteLine($"Hello {name}!");
}
```

Additional information about an option can be provided using the `[Option]` attribute, including an explicit name, aliases, a description, and whether the option is required.

```csharp
[Command(Name = "hello", Description = "Hello example", Hidden = false)]
public static void Hello(
    [Option(Name = "name", Description = "The name we should use for the greeting")]
    string name = "World"
) 
{
    Console.WriteLine($"Hello {name}!");
}
```

**Required options**

Required options must be specified on the command line; otherwise, the program will show an error and display the command help. Method parameters that have default values (as in the examples above) are, by default, translated into options that are not required, while those without default values are always translated into required options. You may force option to be required using `Required` property of the attribute.

**Option name convention**

- The option name is automatically prepended with a single dash (`-`) if it consists of a single letter, or with two dashes (`--`) if it is longer, unless it already starts with a dash.
- If option name is not explicitly specified in the attribute, or attribute is omitted, the  name of the parameter, converted to [kebab case](#kebab-case), will be used implicitly. For example, for the parameter `userId` the default option name will be `--user-id`.
- Options may have [aliases🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#aliases). These are usually short forms that are easier to type or alternate spellings of a word.
- Option names and aliases are [case-sensitive🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#case-sensitivity). If you want your CLI to be case insensitive, define aliases for the various casing alternatives.

**What do we have so far?**

With the full program source code consisting of just a few lines:

```csharp
using SnapCLI;
class Program
{
    [Command(Name = "hello", Description = "Hello example")]
    public static void Hello(
        [Option(Name = "name", Description = "The name we should use for the greeting")]
        string name = "World"
    ) 
    {
        Console.WriteLine($"Hello {name}!");
    }
}
```

We get complete help output:

```text
> sample hello -?
Description:
  Hello example

Usage:
  sample hello [options]

Options:
  --name <name>   The name we should use for the greeting [default: World]
  -?, -h, --help  Show help and usage information
```

We may run the command without a parameter (default name value `World` is used):

```text
> sample hello
Hello World!
```

And we may may run command with the a parameter:

```text
> sample hello --name Michael
Hello Michael!
```

### Arguments

An [argument🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#arguments) is a value passed to an option or command without specifying an option name; it is also referred to as a positional argument.

You can declare that a parameter is an argument using the `[Argument]` attribute. Let's change "Option" to "Argument" in our example:

```csharp
[Command(Name = "hello", Description = "Hello example")]
public static void Hello(
    [Argument(Name = "name", Description = "The name we should use for the greeting")]
    string name = "World"
) 
{
    Console.WriteLine($"Hello {name}!");
}
```

Now we don't need to specify `--name` option name.

```text
> sample hello Michael
Hello Michael!
```

Also, note how the help message has changed:

```text
> sample hello -?
Description:
  Hello example

Usage:
  sample hello [<name>] [options]

Arguments:
  <name>  The name we should use for the greeting [default: World]

Options:
  -?, -h, --help  Show help and usage information
```

**Argument name convention**

- Argument name is used only for help, it cannot be specified on command line.
- If argument name is not explicitly specified in the attribute, the name of the parameter, converted to [kebab case](#kebab-case), will be used implicitly.

You can provide options before arguments or arguments before options on the command line. See [System.CommandLine documentation🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#order-of-options-and-arguments) for details.

## Advanced Usage

In addition to basic constructs for [commands](#commands), [options](#options), and [arguments](#arguments), the library offers fine control over command-line commands and options hierarchy. It also facilitates application [initialization and execution](#application-lifecycle), input [validation](#validation), and [exception handling](#exception-handling).

### Root Command

The [root command🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#root-command) is executed if program invoked without any known commands on the command line. If no handler is assigned for the root command, the CLI will indicate that the required command is not provided and display the help message. To assign a handler method for the root command, use the `[RootCommand]` attribute. Its usage is similar to the `[Command]` attribute, except that you cannot specify a command name. There can be only one method declared with `[RootCommand]` attribute.

The description for the root command essentially serves as the program description in the help output, as shown when program is invoked with the `--help` parameter. If the root command is not declared, `SnapCLI` library will use the assembly description as the root command description.

```csharp
[RootCommand(Description = "This command greets the world!")]
public static void Hello()
{
    Console.WriteLine("Hello World!");
}
```

> **Note**: If a program has only one command handler method declared with the [Command] attribute, and the command name is not explicitly specified in the `Name` property of the attribute, the library will automatically set this command as the root command.

### Subcommands

Any command may have multiple [subcommands🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#subcommands). If command name includes spaces or if the name is not specified and the method name contains underscores, it will describe a subcommand.

In the following example we have a subcommand `world` of the command `hello`:

```csharp
[Command(Name = "hello world", Description = "This command greets the world!")]
public static void Hello() 
{
    Console.WriteLine("Hello World!");
}
```

Or equivalent using just method name:

```csharp
[Command(Description = "This command greets the world!")]
public static void Hello_World() 
{
    Console.WriteLine("Hello World!");
}
```

The usage output will be as follows:

```text
> sample -?
Description:

Usage:
  sample [command] [options]

Options:
  -?, -h, --help  Show help and usage information
  --version       Show version information

Commands:
  hello
```

```text
> sample hello -?
Description:

Usage:
  sample hello [command] [options]

Options:
  -?, -h, --help  Show help and usage information

Commands:
  world
```

```text
> sample hello world -?
Description:
  This command greets the world!

Usage:
  sample hello world [options]

Options:
  -?, -h, --help  Show help and usage information

```

```text
> sample hello world
Hello World!
```

### Commands without Handlers

In the output above we have description for the `hello world` command, but not for the `hello`. To describe the `hello` command without assigning a handler method you may use `[assembly: Command()]` attribute at the top of the program source.

Similarly, you can provide description for the root command (the first description in the output above) using `[assembly: RootCommand()]` attribute.

With descriptions provided as shown in the following example, the help output will be complete.

```csharp
using SnapCLI;

[assembly: RootCommand(Description = "This is a sample program")] // or [assembly: AssemblyDescription("This is sample program")]
[assembly: Command(Name = "hello", Description = "This command greets someone", Aliases= "hi,hola,bonjour")]

class Program
{
    [Command(Description = "This command greets the world!")]
    public static void Hello_World() 
    {
        Console.WriteLine("Hello World!");
    }
}
```

### Recursive Options

A recursive option is available to the command it's assigned to and recursively to all its subcommands. The command may have multiple recursive options along with multiple regular options.

Since recursive options should be available to multiple commands, and to avoid multiple declarations of the same option in multiple places, they are declared with `[Option]` attribute on static properties or fields in separate class, and the class is referenced in `RecursiveOptionsContainingType` property of `[Command]` attribute.

```csharp
class FooRecursiveOptions
{
    [Option(Description="Example recursive option")]
    public static string recursiveOption = "default value #1";

    [Option(Description="Another recursive option")]
    public static string recursiveOption2 {get; set;} = "default value #2";

    // this field will NOT be binded as recursive option because it doesn't have [Option] attribute
    public static string field;
}


[Command(Name="foo", RecursiveOptionsContainingType=typeof(FooRecursiveOptions))]
public static void foo(int opt=0)
{
  Console.WriteLine($"foo: opt={opt}, recursiveOption={FooRecursiveOptions.recursiveOption}, recursiveOption2={FooRecursiveOptions.recursiveOption2}");
}

// the following command can be invoked for example with command line 'app.exe foo --recursiveOption=test subcommand --bar=10'
// the handler has access to recursive options of foo command through FooRecursiveOptions class
[Command(Name="foo subcommand")]
public static void foo_subcommand(int bar=1)
{
  Console.WriteLine($"foo subcommand: bar={bar}, recursiveOption={FooRecursiveOptions.recursiveOption}, recursiveOption2={FooRecursiveOptions.recursiveOption2}");
}
```

By default, recursive options are *not required*, meaning they can be omitted from the command line. This is because properties and fields always have default values, either implicitly or explicitly. It is possible to force a recursive option to be *required* by using the `Required` property of the attribute. The *required* option must be provided by the user when invoking the command.

### Global Options

Global options are essentially recursive options declared at the root command level.

Similar to recursive options, the type for global options can be explicitly specified using the `GlobalOptionsContainingType` property in the `[RootCommand]` attribute.

```csharp
using SnapCLI;
[assembly:RootCommand(GlobalOptionsContainingType=typeof(GlobalOptions))]

class GlobalOptions
{
    [Option(Description="Example global option")]
    public static string globalOption = "default value";
}

class Program
{
    [Command]
    public static void foo(int opt=0)
    {
      Console.WriteLine($"foo: opt={opt}, globalOption={GlobalOptions.globalOption}");
    }
}
```

If the `GlobalOptionsContainingType` is not specified, the default behavior is as follows:

- Any static property or field that has the `[Option]` attribute becomes a global option.
- This behavior excludes properties or fields in classes referenced by `RecursiveOptionsContainingType`.

This approach simplifies application development by automatically identifying global options based on the presence of the `[Option]` attribute, while allowing for customization through the use of `GlobalOptionsContainingType`.

```csharp
class Program
{
    // This global option is not required and have explicit default value of "config.json"
    [Option(Name = "config", Description = "Configuration file name", Aliases = "c,cfg")]
    public static string ConfigFile = "config.json";

    // This global option is not required and have implicit default value of (null)
    [Option(Name = "profile", Description = "User profile")]
    public static string Profile;

    // This global option is always required (must be specified on command line)
    [Option(Name = "user", Description = "User name", Required = true)]
    public static string User { get; set; }

    [Command]
    public static void DoWork(int commandSpecificOption = 0)
    {
        Console.WriteLine($"config: {ConfigFile}, user: {User}, profile: {Profile}, commandSpecificOption: {commandSpecificOption}");
        ...
    }
}
```

### Arity

The [arity🗗](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax#argument-arity) of an option or command's argument is the number of values that can be passed if that option or command is specified. Arity is expressed with a minimum value and a maximum value.

```csharp
[Command(Name = "print", Description = "Arity example")]
public static void Print(
    [Argument(arityMin:1, arityMax:2, Name = "numbers", Description = "Takes 1 or 2 numbers")]
    int[] nums
)
{
    Console.WriteLine($"Numbers are: {string.Join(",", nums)}!");
}
```

<details>
<summary>Sample output</summary>

```text
> sample print -?
Description:
  Arity example

Usage:
  sample print <numbers>... [options]

Arguments:
  <numbers>  Takes 1 or 2 numbers

Options:
  -?, -h, --help  Show help and usage information
```

```text
> sample print 12
Numbers are: 12!
```

```text
> sample print 12 76
Numbers are: 12,76!
```

</details>

### Kebab Case

When the command, option, or argument name is not specified in the attribute, an implicit name is generated based on the method name for commands or the parameter name for options and arguments. If the name is in [Camel case🗗](https://en.wikipedia.org/wiki/Letter_case#Camel_case), a hyphen is inserted between words. Finally, the name is converted to lowercase.

Below are examples of method/parameter names and their resulting kebab-case names.

| Name               | Kebab-case            |
|--------------------|-----------------------|
| `Hello`            | `hello`               |
| `HelloWorld`       | `hello-world`         |
| `user`             | `user`                |
| `userId`           | `user-id`             |
| `noLaunchProfile`  | `no-launch-profile`   |

## Application Lifecycle

Here are the steps of the CLI application initialization and execution lifecycle with the `SnapCLI` library:

1. The `SnapCLI` library entry point is executed (see notes about [Main](#main-method) method).
1. The library scans the assembly for `[RootCommand]`, `[Command]`, `[Option]`, `[Argument]`, and `[Startup]` attributes.
1. The `System.CommandLine` parser is initialized, and the commands hierarchy is built based on the found attributes.
1. The [Startup](#startup) method(s) are executed, if present.
1. The command line is parsed.
1. [Global options](#global-options) are set according to the command line parameters.
1. The [BeforeCommand](#beforecommand) event is invoked.
1. The command handler corresponding to the command specified on the command line is executed.
1. The [AfterCommand](#aftercommand) event is invoked.
1. The process exits.

The command handler is the only *required* component that must be provided by the application developer, all other steps are automatic or optional.

### Main Method

Typically, the `Main` method serves as the entry point for a C# application and is often used to execute startup code.

However, to simplify the process of writing CLI applications, this library overrides the program's entry point, taking control from the very start of application execution. It then parses the command line and calls the appropriate command handler method, treating it as an entry point.

The program may still contain a `Main` method for simple applications, or it may not have a `Main` method at all, as described in the following sections.

#### Parameterized Main

For simple applications that do not involve commands (and thus no command handlers) but only need to parse options/arguments from the command line, the library supports a *parameterized* `Main` method. In this case, method parameters are automatically mapped to the command line options/arguments. Such applications typically contain most of their code directly within the `Main` method.

Example:

```csharp
using SnapCLI;

class Program 
{
    public static void Main([Argument] string arg, int intOption, string strOption = "foo")
    {
        Console.WriteLine($"Argument: {arg}");
        Console.WriteLine($"Options are: {intOption}, {strOption}");
    }
}
```

This approach enables you to create simple CLI tools with minimal effort, providing type-checked options and arguments, as well as basic help functionality out of the box.

A more detailed example is provided in [this tutorial](./Your-First-SnapCLI-App.md).

From a technical perspective, if no command handlers are declared in the program and a `Main` method is present, the library automatically recognizes the `Main` method as the [root command](#root-command) handler.

However, if the `Main` method is used alongside any command handlers, the library will raise an exception. See the next section for further details.

#### Multi-Command Applications

If your CLI program implements multiple [commands](#commands) (i.e., it has multiple handlers declared with `[Command]` and/or `[RootCommand]` attributes), the library will call the appropriate handler depending on the command specified on the command line. You can think of each handler method as a separate entry point into the program, each associated with its corresponding command. See the [sample application](../samples/base64/Program.cs) for an example.

Since the library overrides the program's entry point and the `Main` method is not associated with any command, it will **not** be executed at the start of the program. This can lead to confusion, as some developers may still expect it to run in the traditional way. To avoid this, the library will raise an exception if a `Main` method is present alongside any command handlers declared with the `[Command]` attribute.

Any initialization code that must run before any commands are executed can be placed in the [Startup](#startup) method or the [BeforeCommand](#beforecommand) event handler.

#### Classic Main Behavior

If you need to use your own `Main` method as the first entry point for the application, you can do so by following these steps:

1. Add the `<AutoGenerateEntryPoint>false</AutoGenerateEntryPoint>` property to your program's `.csproj` file.
2. Call `SnapCLI` from your `Main()` method as follows:

```csharp
public static async Task<int> Main(string[] args)
{
    // Your initialization code here
    ...

    return await SnapCLI.CLI.RunAsync(args);
}
```

### Startup

A static method can be declared to perform additional initialization using the `[Startup]` attribute. There could be multiple startup methods in the assembly. These methods will be executed on application start and *before* command line is parsed.

The startup method is recognized by its attribute rather than its name; in other words, you can name it anything you like.

```csharp
[Startup]
public static void MyStartupCode()
{
    // additional initialization for your code
    ...
}
```

The startup method may have a parameter of type `InvocationConfiguration`, which allows configuring runtime behavior such as output streams, error stream, and process termination timeout.

```csharp
[Startup]
public static void Startup(InvocationConfiguration config)
{
    // redirect output to a custom writer
    config.Output = new StreamWriter("output.log");

    // configure Ctrl+C / SIGTERM cancellation timeout
    config.ProcessTerminationTimeout = TimeSpan.FromSeconds(5);
}
```

The `CLI.RootCommand` property, which provides access to the `System.CommandLine` commands hierarchy along with their options and arguments, is available in the startup code for further customization.

SnapCLI registers the `[env:key=value]`, `[diagram]` and `[suggest]` directives on the root command before the startup methods run, and sets `EnableDefaultExceptionHandler` to `false` so that exceptions are routed to the SnapCLI [exception handler](#exception-handling). A startup method may change either — for example, `CLI.RootCommand.Directives.Clear()` removes the directives.

> **Important:** When the startup method is invoked, the command line has not been parsed yet; therefore properties and fields declared as global options still have their default values and not the values from the command line, and `CLI.ParseResult` property is not accessible.

### BeforeCommand

The `BeforeCommand` event is invoked after the command line is parsed and before the command handler is executed. It allows for any additional common initialization, validation of preprocessing the program may need before executing any command. With the command line parameters already parsed, global options reflecting the values specified on the command line and the `CLI.ParseResult` property is accessible for validation or to access parsed options and arguments.

The `BeforeCommand` event handler receives a `BeforeCommandEventArguments` parameter with the following member:
- `ParseResult` - The command line parse result.

To register a `BeforeCommand` event handler, use the following code in startup method:

```csharp
[Startup]
public static void Startup()
{
    CLI.BeforeCommand += (args) => {
        // common initialization or validation before executing any command
        ...
    }
}
```

### AfterCommand

The `AfterCommand` event is invoked after the command handler is executed. It allows for any common deinitialization or post-processing the program may need after executing a command. The event handler receives an `AfterCommandEventArguments` parameter with the following members:

- `ParseResult` - The command line parse result.
- `ExitCode` - The exit code to return from the CLI program. The handler may change the exit code to reflect specific execution results.

> **Note:** `AfterCommand` is invoked even when the command handler threw an exception, so that cleanup always runs. In that case the [exception handler](#exception-handling) has already run and `ExitCode` holds the value it returned.

To register an `AfterCommand` event handler, use the following code:

```csharp
[Startup]
public static void Startup()
{
    CLI.AfterCommand += (args) => {
        // Common deinitialization or post-processing after executing any command
        ...
    };
}
```

## Validation

### Input Validation

There are multiple strategies to validate command line input.

- Input values can be validated at the beginning of the command handler method in any     manner required by the command syntax.

  ```csharp
  [Command]
  public static void command([Argument] int arg1 = 1, int opt1 = 1, int opt2 = 2) 
  {
      if (arg1 < 0 || arg1 > 100)
          throw new CommandLineInputException($"The valid range for the <arg1> value is 0-100");
      ...
  }
  ```

  Throwing `CommandLineInputException` marks the failure as bad user input rather than a program
  fault, so the [default exception handler](#exception-handling) reports it as a short error message
  instead of a stack trace. Any other exception type is still handled, just reported in full.

- For global and recursive options validation may be implemented in property setter.

  ```csharp
  class Program
  {
      // global option with validation
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

      ...
  }
  ```

  > **Note:** the setter is invoked only when the option is actually present on the command line. When
  > it is omitted, the property or field keeps the value its own initializer assigned, and the setter
  > is not called.

- The `SnapCLI.DataAnnotations` library enables validation of command-line arguments using [DataAnnotations🗗](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations) validation attributes. See library [documentation](https://github.com/mikepal2/snap-cli-data-annotations/blob/main/README.md) for details.

  ```csharp
  using SnapCLI;
  using SnapCLI.DataAnnotations;
  using System.ComponentModel.DataAnnotations;

  [Startup]
  public static void Startup()
  {
      // enable validation
      CLI.BeforeCommand += (args) => args.ParseResult.ValidateDataAnnotations();
  }

  [Command(Name = "quotes read", Description = "Read and display the file.")]
  public static async Task ReadQuotesFile(
      [Argument(Name = "file", Description = "File containing quotes")]
      [FileExtensions(Extensions = "txt,quotes")]
      [FileExists]
      FileInfo file,

      [Option(Description = "Delay between lines, specified as milliseconds per character in a line.")]
      [Range(0, 1000)]
      int delay = 42,

      [Option(Name = "fgcolor", Description = "Foreground color of text displayed on the console.")]
      [AllowedValues(ConsoleColor.White, ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.Green)]
      ConsoleColor fgColor = ConsoleColor.White)
  {
      Console.ForegroundColor = fgColor;
      foreach (string line in File.ReadLines(file.FullName))
      {
          Console.WriteLine(line);
          await Task.Delay(delay * line.Length);
      };
  }

  ```

### Mutually Exclusive Options and Arguments

In CLI applications, checking for mutually exclusive options and arguments is a common scenario. This library provides effective mechanisms to perform these checks.

- The `MutuallyExclusiveOptionsArguments` property of the `[Command]` attribute can be used to declare a comma-separated list of mutually exclusive option/argument names. If there are multiple groups of mutually exclusive options/arguments, they must be enclosed in parentheses.

  ```csharp
  [Command(MutuallyExclusiveOptionsArguments = "(opt1,opt2)(arg1,opt2)")]
  public static void command([Argument] int arg1 = 1, int opt1 = 1, int opt2 = 2) 
  {
      ...
  }
  ```

* The `ParseResult.ValidateMutuallyExclusiveOptionsArguments()` method can be used from within the command handler method.

  ```csharp
  [Command]
  public static void command([Argument] int arg1 = 1, int opt1 = 1, int opt2 = 2) 
  {
      CLI.ParseResult.ValidateMutuallyExclusiveOptionsArguments("(opt1,opt2)(arg1,opt2)");
      ...
  }
  ```

* Alternatively, the `ParseResult.ValidateMutuallyExclusiveOptionsArguments()` method can be used from the [BeforeCommand](#beforecommand) event handler.

  ```csharp
  [Startup]
  public static void Startup()
  {
      CLI.BeforeCommand += (args) => {
          args.ParseResult.ValidateMutuallyExclusiveOptionsArguments("global-opt1,global-opt2");
          args.ParseResult.ValidateMutuallyExclusiveOptionsArguments("(global-opt1,opt2)(opt3,opt4)");
          ...
      };
  }
  ```

When a violation is detected, a `CommandLineInputException` is thrown; the default exception handler
reports it as a short error message and exits with code 1. Names that do not match any option or
argument reachable from the command are rejected with an `AttributeUsageException`, so a typo in the
list cannot silently disable the check.

## Exception Handling

To catch unhandled exceptions during command execution you may set exception handler in [Startup](#startup) method. The handler is intended to provide exception diagnostics according to the need of your application before exiting. The return value from handler will be used as program's exit code. For example:

```csharp
[Startup]
public static void Startup()
{
    CLI.ExceptionHandler = (exception) => {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        if (exception is OperationCanceledException)
        {   // special case
            Console.Error.WriteLine("Operation cancelled!");
        }
        else if (g_debugMode)
        {   // show detailed exception info in debug mode
            Console.Error.WriteLine(exception.ToString());
        }
        else 
        {   // show short error message during normal run
            Console.Error.WriteLine($"Error: {exception.Message}");
        }
        Console.ForegroundColor = color;
        return 1; // exit code
    };
}
```

The handler covers the whole run, not just the command handler body: building the commands hierarchy
from attributes, parsing the command line, binding [global options](#global-options) (including any
validation in their property setters), and executing the command. Setting `CLI.ExceptionHandler` to
`null` suppresses handling and lets exceptions propagate out of `CLI.Run()` / `CLI.RunAsync()`.

The default handler prints the full exception, except for two cases it reports as a single
`Error: <message>` line because a stack trace adds nothing:

- `CommandLineInputException` - invalid input on the command line.
- `AttributeUsageException` - incorrect use of the SnapCLI attributes, detected while building the commands hierarchy.

`OperationCanceledException` is treated as cancellation and reported silently.

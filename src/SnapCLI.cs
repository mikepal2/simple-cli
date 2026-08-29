using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SnapCLI
{
    /// <summary>
    /// Declares an <see cref = "Option"/> for CLI command.
    /// <remarks>
    /// <para>Applies to command handler method arguments.</para>
    /// <para>Can also be used on static fields and properies to declare global options.</para>
    /// </remarks>
    /// <example>
    /// <code>
    ///
    ///     [Command]
    ///     public static void Hello(
    ///         [Option(Name = "name", Description = "Person's name")]
    ///         string personName = "everyone"
    ///     )
    ///     {
    ///       Console.WriteLine($"Hello {personName}!");
    ///     }
    ///
    /// </code>
    ///
    /// Global option:
    /// <code>
    ///
    ///     [Option(Name = "config", Description = "Specifies configuration file path")]
    ///     public static string g_configFile = "config.json";
    ///
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field)]
    public class OptionAttribute : Attribute
    {
        /// <summary>
        /// Declares an <see cref = "Option"/>.
        /// </summary>
        public OptionAttribute()
        {
        }

        /// <summary>
        /// Declares an <see cref = "Option"/> with arity.
        /// </summary>
        /// <param name = "arityMin">The minimum number of values an option can receive.</param>
        /// <param name = "arityMax">The maximum number of values an option can receive.</param>
        public OptionAttribute(int arityMin, int arityMax)
        {
            Arity = new ArgumentArity(arityMin, arityMax);
        }

        /// <summary>
        /// The name of the option.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// A description of the option.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// A comma-separated list of aliases for the option.
        /// </summary>
        public string? Aliases { get; set; }

        /// <summary>
        /// The name of the option value.
        /// </summary>
        public string? HelpName { get; set; }

        /// <summary>
        /// Hidden options are not shown in help, but they can still be used on the command line.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Required options must be specified on the command line, otherwise an error is reported.
        /// Parameters without a default value are required implicitly; setting this property forces
        /// an option to be required even when a default value is available.
        /// </summary>
        public bool Required { get; set; }

        internal ArgumentArity? Arity { get; }
    }

    /// <summary>
    /// Declares an <see cref = "Argument"/> for CLI command.
    /// <remarks>
    /// <para>Can be applied to method argument</para>
    /// </remarks>
    /// <example>
    /// <code>
    ///
    ///     [Command]
    ///     static public void Read(
    ///         [Argument(Name = "path", Description = "Input file path")]
    ///         string filepath
    ///     )
    ///     {
    ///       ...
    ///     }
    ///
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ArgumentAttribute : Attribute
    {
        /// <summary>
        /// Declares an <see cref = "Argument"/>.
        /// </summary>
        public ArgumentAttribute()
        {
        }

        /// <summary>
        /// Declares an <see cref = "Argument"/> with arity.
        /// </summary>
        /// <param name = "arityMin">The minimum number of values the argument can receive.</param>
        /// <param name = "arityMax">The maximum number of values the argument can receive.</param>
        public ArgumentAttribute(int arityMin, int arityMax)
        {
            Arity = new ArgumentArity(arityMin, arityMax);
        }

        /// <summary>
        /// The name of the argument.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// A description of the argument.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The name of the argument value.
        /// </summary>
        public string? HelpName { get; set; }

        /// <summary>
        /// Hidden arguments are not shown in help, but they can still be used on the command line.
        /// </summary>
        public bool Hidden { get; set; }

        internal ArgumentArity? Arity { get; }
    }

    /// <summary>
    /// Declares <see cref = "RootCommand"/>, i.e. command that executed when no subcommands are present on the command line.
    /// Only one method may be declared with this attribute.
    /// <remarks>
    /// <para>If program has only one method declared with <see cref = "CommandAttribute"/> and command name not explicitly specified in <c>name</c> attribute, this command is automatically treated as root command.</para>
    /// </remarks>
    /// <example>
    /// <code>
    ///
    ///     [RootCommand]
    ///     static public void Hello()
    ///     {
    ///       ...
    ///     }
    ///
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Assembly)]
    public class RootCommandAttribute : Attribute
    {
        /// <summary>
        /// A description of the command.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// A comma-separated list of mutually exclusive options/arguments names. If there are multiple groups of mutually exclusive options/arguments, they must be enclosed in parentheses. Example: (option1,option2)(option3,arg1)
        /// </summary>
        public string? MutuallyExclusiveOptionsArguments { get; set; }

        /// <summary>
        /// Specifies the type (class) containing static properties and/or fields declared with the [Option] attribute to be added as global options at the root command level.
        /// </summary>
        /// <remarks>
        /// By default, all static properties and fields declared as options are automatically added global, unless their containing type is specified in <see cref="CommandAttribute.RecursiveOptionsContainingType"/>.
        /// The GlobalOptionsContainingType allows to specify global options containing type explicitly.
        /// </remarks>
        public Type? GlobalOptionsContainingType { get; set; }
    }

    /// <summary>
    /// Declares CLI <see cref = "Command"/>.
    /// <remarks>
    /// <para>Can be applied to any static public method</para>
    /// <para>If program has only one method declared with <see cref = "CommandAttribute"/> and command name not explicitly specified with <see cref="Name"/> property, this command is automatically treated as root command.</para>
    /// </remarks>
    /// <example>
    /// <code>
    ///
    ///     [Command]
    ///     static public void Hello()
    ///     {
    ///       ...
    ///     }
    ///
    /// </code></example>
    /// </summary>
    /// <remarks>
    /// <list type = "bullet">
    /// <item><description>If the name is not specified and the program has only one method declared with <see cref = "CommandAttribute"/>, this command is automatically treated as <see cref = "RootCommand"/>.</description></item>
    /// <item><description>If the name is not specified, the method name converted to kebab-case is used as the command name. For example, the method <c>HelloWorld()</c> will handle the <c>hello-world</c> command.</description></item>
    /// <item><description>If the method name is used and contains the underscore <c>_</c> character, it describes a subcommand. For example, the method <c>order_create()</c> is the subcommand <b>crate</b> of the <b>oreder</b> command.</description></item>
    /// <item><description>If the name is specified and contains spaces, it describes a subcommand. For example, <c>order list</c> is the subcommand <b>list</b> of the <b>order</b> command.</description></item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// The name of the command.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// A description of the command.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// A comma-separated list of aliases for the command.
        /// </summary>
        public string? Aliases { get; set; }

        /// <summary>
        /// Hidden commands are not shown in help, but they can still be used on the command line.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// A comma-separated list of mutually exclusive options/arguments names. If there are multiple groups of mutually exclusive options/arguments, they must be enclosed in parentheses. Example: (option1,option2)(option3,arg1)
        /// </summary>
        public string? MutuallyExclusiveOptionsArguments { get; set; }

        /// <summary>
        /// Specifies type (class) containing static properties and/or fields declared with [Option] attribute to be added as recursive options for the command.
        /// </summary>
        public Type? RecursiveOptionsContainingType { get; set; }
    }

    /// <summary>
    /// This exception describes incorrect usage of CLI attributes. It is thrown during CLI initialization if any errors in attribute usage are detected.
    /// </summary>
    public class AttributeUsageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the SnapCLI.AttributeUsageException class.
        /// </summary>
        public AttributeUsageException() : base() { }
        /// <summary>
        /// Initializes a new instance of the SnapCLI.AttributeUsageException class with a specified error message.
        /// </summary>
        public AttributeUsageException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the SnapCLI.AttributeUsageException class with a specified error message and inner exception.
        /// </summary>
        public AttributeUsageException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Describes invalid input on the command line, as opposed to a failure inside the program.
    /// The default exception handler reports these as a short error message without a stack trace.
    /// </summary>
    /// <remarks>
    /// Derives from <see cref = "ArgumentException"/> so that existing <c>catch (ArgumentException)</c> handlers keep working.
    /// </remarks>
    public class CommandLineInputException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the SnapCLI.CommandLineInputException class.
        /// </summary>
        public CommandLineInputException() : base() { }
        /// <summary>
        /// Initializes a new instance of the SnapCLI.CommandLineInputException class with a specified error message.
        /// </summary>
        public CommandLineInputException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the SnapCLI.CommandLineInputException class with a specified error message and inner exception.
        /// </summary>
        public CommandLineInputException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Declares startup method for CLI. The method must be public static and may have no parameters or one parameter of type <see cref = "InvocationConfiguration"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class StartupAttribute : Attribute
    {
    }

    /// <summary>
    /// Command Line Interface class implementation. Provides simple interface to create CLI programs using attributes declarations.
    /// </summary>
    public static class CLI
    {
        /// <summary>
        /// Arguments for the BeforeCommand event.
        /// </summary>
        public class BeforeCommandEventArguments
        {
            /// <summary>
            /// Command line parse result.
            /// </summary>
            public ParseResult ParseResult;

            /// <summary>
            /// BeforeCommandEventArguments constructor.
            /// </summary>
            /// <param name = "parseResult">Command line parse result.</param>
            public BeforeCommandEventArguments(ParseResult parseResult) => ParseResult = parseResult;
        }

        /// <summary>
        /// Event invoked immediately before command is executed. Can be used for custom initialization.
        /// </summary>
        public static event Action<BeforeCommandEventArguments>? BeforeCommand;

        /// <summary>
        /// Arguments for AfterCommandEvent.
        /// </summary>
        public class AfterCommandEventArguments
        {
            /// <summary>
            /// Command line parse result.
            /// </summary>
            public ParseResult ParseResult;

            /// <summary>
            /// Exit code to return from CLI program. The handler may change the exit code to reflect specific execution results.
            /// </summary>
            public int ExitCode;

            /// <summary>
            /// AfterCommandEventArguments constructor.
            /// </summary>
            /// <param name = "parseResult">Command line parse result.</param>
            /// <param name = "exitCode">Exit code to return from CLI program.</param>
            public AfterCommandEventArguments(ParseResult parseResult, int exitCode)
            {
                ParseResult = parseResult;
                ExitCode = exitCode;
            }

        }

        /// <summary>
        /// Event invoked immediately after command was executed. Can be used for deinitialization.
        /// </summary>
        public static event Action<AfterCommandEventArguments>? AfterCommand;

        /// <summary>
        /// Provides access to commands hierarchy and their options and arguments.
        /// </summary>
        /// <exception cref = "AttributeUsageException">Attribute usage error detected while building the commands hierarchy.</exception>
        public static RootCommand RootCommand
        {
            get
            {
                Initialize();
                return _rootCommand!;
            }
        }
        private static RootCommand? _rootCommand;

        /// <summary>
        /// Handler to use when exception is occured during command execution. Set <code>null</code> to suppress exception handling.
        /// </summary>
        /// <returns>Returns exit code to return from program. It is strongly recommanded to return non-zero exit code on error.</returns>
        public static Func<Exception, int>? ExceptionHandler { get; set; } = DefaultExceptionHandler;

        /// <summary>
        /// Command line parse result.
        /// </summary>
        public static ParseResult ParseResult
        {
            get { return _parseResult ?? throw new InvalidOperationException($"The {nameof(ParseResult)} property is not available before command line is parsed."); }
            private set { _parseResult = value; }
        }
        private static ParseResult? _parseResult;

        /// <summary>
        /// Cancellation token for asynchronous CLI commands.
        /// </summary>
        public static CancellationToken CancellationToken { get; private set; } = CancellationToken.None;

        private static TextWriter Error
        {
            get => _error ?? Console.Error;
            set => _error = value;
        }
        private static TextWriter? _error;

        private static InvocationConfiguration _defaultInvocationConfig = new InvocationConfiguration();

        private static (int exitCode, Exception? toRethrow) InvokeExceptionHandler(Exception ex)
        {
            if (ExceptionHandler != null)
            {
                try { return (ExceptionHandler(ex), null); }
                catch (Exception handlerEx) { return (1, handlerEx); }
            }
            return (1, ex);
        }

        private static int DefaultExceptionHandler(Exception exception)
        {
            switch (exception)
            {
                case OperationCanceledException _:
                    break;

                // Bad input from the user and attribute misuse by the developer are both actionable
                // without a stack trace, so report just the message.
                case CommandLineInputException _:
                case AttributeUsageException _:
                    WriteError($"Error: {exception.Message}");
                    break;

                default:
                    WriteError(exception.ToString());
                    break;
            }

            return 1;

            static void WriteError(string text)
            {
                if (Error == Console.Error)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Error.WriteLine(text);
                    Console.ForegroundColor = color;
                }
                else
                {
                    Error.WriteLine(text);
                }
            }
        }

        private class CommandDescriptor
        {
            public readonly string? SpecifiedName;
            public readonly string CommandName;
            public readonly string? Description;
            public readonly Attribute Attribute;
            public readonly MethodInfo? Method;
            public readonly string? MutuallyExclusiveOptionsArguments;
            public readonly Type? RecursiveOptionsContainingType;

            public CommandDescriptor(CommandAttribute attribute, MethodInfo? method)
            {
                Method = method;
                Attribute = attribute;
                SpecifiedName = attribute.Name;
                CommandName = attribute.Name
                    ?? method?.Name.Replace('_', ' ').ToKebabCase()
                    ?? throw new AttributeUsageException($"[Command] attribute declared at assembly level must have Name property specified");
                MutuallyExclusiveOptionsArguments = attribute.MutuallyExclusiveOptionsArguments;
                Description = attribute.Description;
                RecursiveOptionsContainingType = attribute.RecursiveOptionsContainingType;
            }

            public CommandDescriptor(RootCommandAttribute attribute, MethodInfo? method)
            {
                Method = method;
                Attribute = attribute;
                MutuallyExclusiveOptionsArguments = attribute.MutuallyExclusiveOptionsArguments;
                Description = attribute.Description;
                CommandName = RootCommand.ExecutableName;
                RecursiveOptionsContainingType = attribute.GlobalOptionsContainingType;
            }
        }

        /// <summary>
        /// Helper method to run CLI application. Should be called from program Main() entry point.
        /// </summary>
        /// <param name = "args">Command line arguments passed from Main()</param>
        /// <param name = "output">Redirect output stream</param>
        /// <param name = "error">Redirect error stream</param>
        /// <returns></returns>
        public static int Run(string[]? args = null, TextWriter? output = null, TextWriter? error = null)
        {
            _error = error;
            try
            {
                Initialize();
                var parseResult = RootCommand.Parse(args ?? Environment.GetCommandLineArgs().Skip(1).ToArray());
                return parseResult.Invoke(CreateInvocationConfig(output, error));
            }
            catch (Exception ex)
            {
                // Covers failures raised outside the command action: building the commands hierarchy,
                // tokenizing/parsing, and default value factories evaluated during parsing.
                var (exitCode, toRethrow) = InvokeExceptionHandler(ex);
                if (toRethrow != null)
                    ExceptionDispatchInfo.Capture(toRethrow).Throw();
                return exitCode;
            }
        }

        /// <summary>
        /// Helper asynchronous method to run CLI application. Should be called from program async Main() entry point.
        /// </summary>
        /// <param name = "args">Command line arguments passed from Main()</param>
        /// <param name = "output">Redirect output stream</param>
        /// <param name = "error">Redirect error stream</param>
        /// <returns></returns>
        public static async Task<int> RunAsync(string[]? args = null, TextWriter? output = null, TextWriter? error = null)
        {
            _error = error;
            try
            {
                Initialize();
                var parseResult = RootCommand.Parse(args ?? Environment.GetCommandLineArgs().Skip(1).ToArray());
                return await parseResult.InvokeAsync(CreateInvocationConfig(output, error), CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Covers failures raised outside the command action: building the commands hierarchy,
                // tokenizing/parsing, and default value factories evaluated during parsing.
                var (exitCode, toRethrow) = InvokeExceptionHandler(ex);
                if (toRethrow != null)
                    ExceptionDispatchInfo.Capture(toRethrow).Throw();
                return exitCode;
            }
        }

        private static InvocationConfiguration CreateInvocationConfig(TextWriter? output, TextWriter? error)
        {
            if (output == null && error == null)
                return _defaultInvocationConfig;

            // Shallow-copy every writable property so that anything a [Startup] method configured is
            // preserved, then apply the caller's stream redirections on top.
            var config = new InvocationConfiguration();
            foreach (var prop in typeof(InvocationConfiguration).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (prop.CanRead && prop.CanWrite)
                    prop.SetValue(config, prop.GetValue(_defaultInvocationConfig));

            if (output != null) config.Output = output;
            if (error != null) config.Error = error;
            return config;
        }

        private static Dictionary<object, object> _bindings = new Dictionary<object, object>();

        /// <summary>
        /// The library using attributes on methods, properies, fields and parameters to create CommandLine parser commands, options and arguments.
        /// This method returns corresponding entity info (method, propery, field ot parameter) binded to specified CommandLine object.
        /// </summary>
        /// <param name = "commandLineObject">One of CommandLine parser objects: <see cref = "Command"/>, <see cref = "Option"/> or <see cref = "Argument"/>.</param>
        /// <returns>The entity binded to CommandLine parser object <see cref = "MethodInfo"/>, <see cref = "PropertyInfo"/>, <see cref = "FieldInfo"/>, <see cref = "ParameterInfo"/> or <see cref="Assembly"/>. Returns <code>null</code> if binding not found.</returns>
        public static object? GetBinding(object commandLineObject)
        {
            if (_bindings.TryGetValue(commandLineObject, out var binding))
                return binding;
            else
                return null;
        }

        /// <summary>
        /// The library using attributes on methods, properies, fields and parameters to create CommandLine parser commands, options and arguments.
        /// This method returns <see cref = "ICustomAttributeProvider"/> for corresponding entity (method, propery, field ot parameter) binded to specified CommandLine object.
        /// Return binding CommandLine parser object to
        /// </summary>
        /// <param name = "commandLineObject">One of CommandLine parser objects: <see cref = "Command"/>, <see cref = "Option"/> or <see cref = "Argument"/>.</param>
        /// <returns>The entity binded to CommandLine parser object <see cref = "MethodInfo"/>, <see cref = "PropertyInfo"/>, <see cref = "FieldInfo"/> or <see cref = "ParameterInfo"/>. Returns <code>null</code> if binding not found.</returns>
        public static ICustomAttributeProvider? GetBindingCustomAttributeProvider(object commandLineObject)
        {
            return GetBinding(commandLineObject) as ICustomAttributeProvider;
        }

        // all recursive options, i.e. properties and fields described with [Option] attribute
        private static List<RecursiveOption> recursiveOptions = new List<RecursiveOption>();

        private class RecursiveOption
        {
            public readonly Option CliOption;
            public readonly MemberInfo Binding;
            public readonly Type ContainingType;

            public RecursiveOption(Type containingType, OptionAttribute attr, MemberInfo memberInfo)
            {
                switch (memberInfo)
                {
                    case PropertyInfo prop:
                        if (!prop.CanWrite)
                            throw new AttributeUsageException($"Property {prop.Name} declared as [Option] must be writable");
                        if (prop.GetMethod?.IsStatic != true || prop.SetMethod?.IsStatic != true)
                            throw new AttributeUsageException($"Property {GetFullTypeName(containingType)}.{prop.Name} declared as [Option] must be static");
                        CliOption = CreateOption(attr, prop.Name, prop.PropertyType, () => prop.GetValue(null));
                        break;
                    case FieldInfo field:
                        if (field.IsInitOnly)
                            throw new AttributeUsageException($"Field {field.Name} declared as [Option] must be writable");
                        if (!field.IsStatic)
                            throw new AttributeUsageException($"Field {field.Name} declared as [Option] must be static");
                        CliOption = CreateOption(attr, field.Name, field.FieldType, () => field.GetValue(null));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                ContainingType = containingType;
                Binding = memberInfo;
                _bindings.Add(CliOption, Binding);
            }

            public void SetValueFromCommandLine()
            {
                var optionResult = ParseResult.GetResult(CliOption);

                // System.CommandLine produces an implicit result carrying the option's default value even
                // when the option is absent from the command line. Writing that back would re-run the
                // property setter (and any validation in it) on every invocation, so skip it and leave the
                // property or field holding the value its own initializer gave it.
                if (optionResult == null || optionResult.Implicit)
                    return;

                var value = GetValueOrDefault(optionResult, CliOption.ValueType);
                switch (Binding)
                {
                    case PropertyInfo prop:
                        prop.SetValue(null, value);
                        break;
                    case FieldInfo field:
                        field.SetValue(null, value);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            public override string ToString()
            {
                string result;
                string containingTypeName = GetFullTypeName(ContainingType);
                switch (Binding)
                {
                    case PropertyInfo prop:
                        result = $"Property {containingTypeName}:{prop.Name}";
                        break;
                    case FieldInfo field:
                        result = $"Field {containingTypeName}:{field.Name}";
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return result;
            }
        };

        // Initialization state. The commands hierarchy is built lazily rather than from a static
        // constructor: a failing static constructor would surface every attribute usage error as an
        // opaque TypeInitializationException that callers cannot intercept, because touching any
        // member of CLI (including ExceptionHandler) re-throws it.
        private static bool _initializing;
        private static bool _initialized;

        /// <summary>
        /// Builds the commands hierarchy from the attributes declared in the entry assembly.
        /// Called automatically by <see cref = "Run"/>, <see cref = "RunAsync"/> and <see cref = "RootCommand"/>;
        /// calling it explicitly is only useful to surface attribute usage errors early.
        /// </summary>
        /// <exception cref = "AttributeUsageException">Attribute usage error detected.</exception>
        public static void Initialize()
        {
            if (_initialized || _initializing)
                return;
            _initializing = true;
            try
            {
                BuildCommandsHierarchy();
                _initialized = true;
            }
            finally
            {
                _initializing = false;
            }
        }

        static CLI()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (ExceptionHandler != null && args.ExceptionObject is Exception ex)
                    Environment.ExitCode = ExceptionHandler.Invoke(ex);
            };
        }

        private static void BuildCommandsHierarchy()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Assembly assembly = Assembly.GetEntryAssembly() ?? executingAssembly;

            BindingFlags bindingFlags = BindingFlags.Public |
                                 BindingFlags.NonPublic |
                                 BindingFlags.Static |
                                 BindingFlags.Instance |
                                 BindingFlags.DeclaredOnly;

            // find all commands, i.e. all [Command] and [RootCommand] attributes

            if (!GetCommands(assembly, bindingFlags, out var commandDescriptors, out var rootCommandDescriptor))
            {

                // special case: test project calling CLI.Run() directly from test assembly, so we need to search commands in calling assembly
                foreach (var callingAssembly in new StackTrace(1, false)
                                                        .GetFrames()
                                                        .Select(f => f.GetMethod()?.Module?.Assembly)
                                                        .Where(a => a != null && a != executingAssembly)
                                                        .Distinct())
                {
                    if (callingAssembly != null)
                    {
                        assembly = callingAssembly;
                        if (GetCommands(assembly, bindingFlags, out commandDescriptors, out rootCommandDescriptor))
                            break;
                    }
                }
            }

            if (commandDescriptors.Count == 0 && rootCommandDescriptor == null)
                throw new AttributeUsageException("The CLI program must declare at least one method with [Command] or [RootCommand] attribute, see documentation https://github.com/mikepal2/snap-cli/blob/main/README.md");

            // find all recursive options, i.e. properties and fields described with [Option] attribute
            recursiveOptions = assembly.GetTypes().SelectMany(t => t.GetMembers(bindingFlags)
                    .Where(x => x.IsDefined(typeof(OptionAttribute)))
                    .Select(x => new RecursiveOption(t, GetCustomAttribute<MemberInfo, OptionAttribute>(x)!, x)))
                    .ToList();

            // lookup for unreferenced global/recursive options and trace warning about them
            if (rootCommandDescriptor?.RecursiveOptionsContainingType != null)
            {
                foreach (var t in recursiveOptions
                    .Select(o => o.ContainingType)
                    .Distinct()
                    .Where(t => t != rootCommandDescriptor.RecursiveOptionsContainingType && !commandDescriptors.Any(d => d.RecursiveOptionsContainingType == t)))
                {
                    var message = $"The type '{GetFullTypeName(t)}' contains fields declared as [Option] but is not referenced as GlobalOptionsContainingType or RecursiveOptionsContainingType";
                    Trace.TraceWarning(message);
                    //throw new AttributeUsageException(message);
                }
            }

            // create root command

            string rootCommadDescription = rootCommandDescriptor?.Description
                ?? GetCustomAttribute<Assembly, AssemblyDescriptionAttribute>(assembly)?.Description
                ?? "";
            var rootCommand = new RootCommand(rootCommadDescription);
            _rootCommand = rootCommand;

            if (rootCommandDescriptor?.Method != null)
            {
                AddCommandHandler(rootCommand, rootCommandDescriptor.Method, rootCommandDescriptor.MutuallyExclusiveOptionsArguments);
                _bindings.Add(rootCommand, rootCommandDescriptor.Method);
            }
            else
            {
                _bindings.Add(rootCommand, assembly);
            }

            var globalOptions = rootCommandDescriptor?.RecursiveOptionsContainingType != null ?
                recursiveOptions.Where(x => x.ContainingType == rootCommandDescriptor.RecursiveOptionsContainingType) : // GlobalOptionsContainingType was specified explicitly
                recursiveOptions.Where(x => !commandDescriptors.Any(d => d.RecursiveOptionsContainingType == x.ContainingType)); // use all options from types not specified as RecursiveOptionsContainingType

            foreach (var opt in globalOptions)
            {
                opt.CliOption.Recursive = true;
                rootCommand.Add(opt.CliOption);
            }

            // add subcommands

            var subcommands = commandDescriptors
                .OrderBy(desc => desc.CommandName.Length) // sort by name length to ensure parent commands created before subcommands
                .Select(desc => CreateCommand(rootCommand, desc))
                .ToArray();

            // validate subcommands

            foreach (var command in subcommands)
                if (command.Subcommands.Count == 0 && command.Action == null && command.Hidden == false)
                    throw new AttributeUsageException($"Command '{command.Name}' has no subcommands nor handler methods");

            // System.CommandLine builds a single token table from the root command name and all of its
            // immediate subcommand names, so a top level command that shadows the executable name makes
            // every parse throw an obscure duplicate key error. Report it here instead.
            foreach (var command in rootCommand.Subcommands)
                foreach (var token in new[] { command.Name }.Concat(command.Aliases))
                    if (string.Equals(token, RootCommand.ExecutableName, StringComparison.Ordinal))
                        throw new AttributeUsageException(
                            $"Command '{token}' has the same name as the executable '{RootCommand.ExecutableName}'. " +
                            "A top level command cannot shadow the executable name; rename the command or the assembly (see <AssemblyName> in the project file).");

            // set up default invocation configuration — SnapCLI handles exceptions itself
            _defaultInvocationConfig = new InvocationConfiguration
            {
                EnableDefaultExceptionHandler = false
            };

            // Always add standard directives
            rootCommand.Add(new EnvironmentVariablesDirective());
            rootCommand.Add(new DiagramDirective());
            rootCommand.Add(new SuggestDirective());

            // call [Startup] methods — parameterless or accepting InvocationConfiguration
            var startupMethods = GetCallbackMethodsByAttribute<StartupAttribute>(assembly, bindingFlags, paramTypes: new Type[] { typeof(InvocationConfiguration) }, paramsAreOptional: true);

            if (startupMethods.Any())
            {
                foreach (var method in startupMethods)
                {
                    try
                    {
                        if (method.GetParameters().Length > 0)
                            method.Invoke(null, new object[] { _defaultInvocationConfig });
                        else
                            method.Invoke(null, null);
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException != null)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    }
                }
            }
        }

        // find all declared [RootCommand] and [Command] attributes
        private static bool GetCommands(Assembly assembly, BindingFlags bindingFlags, out List<CommandDescriptor> commands, out CommandDescriptor? rootCommand)
        {
            commands = new List<CommandDescriptor>();
            rootCommand = null;
            var mainMethods = new List<MethodInfo>();

            var rootCommandAttribute = GetCustomAttribute<Assembly, RootCommandAttribute>(assembly);
            if (rootCommandAttribute != null)
                rootCommand = new CommandDescriptor(rootCommandAttribute, method: null);

            foreach (var commandAttribute in GetCustomAttributes<Assembly, CommandAttribute>(assembly))
                commands.Add(new CommandDescriptor(commandAttribute, method: null));

            foreach (var method in assembly.GetTypes().SelectMany(t => t.GetMethods(bindingFlags)))
            {
                rootCommandAttribute = GetCustomAttribute<MethodInfo, RootCommandAttribute>(method);
                if (rootCommandAttribute != null)
                {
                    if (rootCommand != null)
                        throw new AttributeUsageException($"Only one [RootCommand] attribute may be declared, found second on method {method.Name}");
                    rootCommand = new CommandDescriptor(rootCommandAttribute, method);
                }

                var commandAttributes = GetCustomAttributes<MethodInfo, CommandAttribute>(method).ToArray();
                if (commandAttributes.Length == 0)
                {
                    if (method.IsStatic && method.Name == "Main"
                        && rootCommand?.Method != method
                        && !(new StackTrace(1, true)).GetFrames().Any(f => f.GetMethod() == method) // skip methods from current stack to support call from "classic" Main
                        )
                    {
                        var declaringTypeName = method.DeclaringType?.FullName;
                        if (declaringTypeName != null &&
                            !declaringTypeName.Contains("TestHost") &&
                            !declaringTypeName.Contains("AutoGenerated")
                        )
                        {
                            mainMethods.Add(method);
                        }
                    }
                    continue;
                }
                if (commandAttributes.Length > 1)
                    throw new AttributeUsageException($"Method {method.Name} has multiple [Command] attributes declared");
                if (rootCommandAttribute != null)
                    throw new AttributeUsageException($"Method {method.Name} has both [Command] and [RootCommand] attributes declared");

                commands.Add(new CommandDescriptor(commandAttributes[0], method));
            }

            // If program has only one method declared with [Command] attribute and command name is not explicitly specified in attribute Name property,
            // this command is automatically treated as root command.
            if (rootCommand == null && commands.Count == 1 && commands.First().SpecifiedName == null)
            {
                rootCommand = commands.First();
                commands.Clear();
            }

            // If program has no methods declared with [Command] or [RootCommand] attributes,
            // then the Main() method is automatically treated as root command handler.
            // NOTE: a Main() method that is present alongside command handlers is an error, see below.
            if (mainMethods.Count > 0)
            {
                if (mainMethods.Count > 1)
                    throw new Exception($"Assembly contains multiple Main() methods");
                if (rootCommand == null && commands.Count == 0)
                    rootCommand = new CommandDescriptor(new RootCommandAttribute(), mainMethods.First());
                else
                    throw new AttributeUsageException("The program has both [Command]/[RootCommand] handlers and Main() method, the Main() method will not be executed");
            }

            return commands.Count > 0 || rootCommand != null;
        }

        // Helper method to provide meaningful diagnostics on attribute instantiation error.
        private static AttrType? GetCustomAttribute<SourceType, AttrType>(SourceType source)
            where SourceType : ICustomAttributeProvider
            where AttrType : Attribute
        {
            try
            {
                return source.GetCustomAttributes(typeof(AttrType), false).OfType<AttrType>().FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to process attributes for {source}", ex);
            }
        }

        // Helper method to provide meaningful diagnostics on attribute instantiation error.
        private static IEnumerable<AttrType> GetCustomAttributes<SourceType, AttrType>(SourceType source)
            where SourceType : ICustomAttributeProvider
            where AttrType : Attribute
        {
            try
            {
                return source.GetCustomAttributes(typeof(AttrType), false).OfType<AttrType>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to process attributes for {source}", ex);
            }
        }

        // find methods declared with attribute
        private static MethodInfo[] GetCallbackMethodsByAttribute<T>(Assembly assembly, BindingFlags bindingFlags, Type[] paramTypes, bool paramsAreOptional = false) where T : Attribute
        {
            var methods = assembly.GetTypes()
                .SelectMany(t => t.GetMethods(bindingFlags))
                .Where(m => GetCustomAttribute<MethodInfo, T>(m) != null)
                .ToArray();

            foreach (var method in methods)
            {
                var attributeName = typeof(T).Name.Replace("Attribute", "");
                if (!method.IsStatic)
                    throw new AttributeUsageException($"Method {method.Name} declared as [{attributeName}] must be static");

                if (!ValidateParams(method, paramTypes, paramsAreOptional))
                {
                    var methodDefinition = $"public static void {method.Name}(";
                    methodDefinition += string.Join(", ", paramTypes.Select(t => (paramsAreOptional ? "[" : "") + t.Name));
                    methodDefinition += paramsAreOptional ? new string(']', paramTypes.Length) : "";
                    methodDefinition += " { ... }";
                    throw new AttributeUsageException($"Method {method.Name} declared as [{attributeName}] must be of type: {methodDefinition}");
                }
            }

            return methods;

            static bool ValidateParams(MethodInfo method, Type[] paramTypes, bool paramsAreOptional)
            {
                var _params = method.GetParameters();
                if (!paramsAreOptional && _params.Length != paramTypes.Length)
                    return false;
                if (_params.Length > paramTypes.Length)
                    return false;
                for (int i = 0; i < _params.Length; i++)
                    if (_params[i].ParameterType != paramTypes[i])
                        return false;
                return true;
            }
        }

        private static Command CreateCommand(RootCommand rootCommand, CommandDescriptor desc)
        {
            if (!(desc.Attribute is CommandAttribute attr))
                throw new ArgumentException($"Unexpected descriptor type {desc.Attribute.GetType()} for the command");

            var name = desc.CommandName;
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Valid command name is required");

            var subcommandNames = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Command parentCommand = rootCommand;
            Command? command = null;
            bool created = false;
            foreach (var (subName, i) in subcommandNames.Select((n, i) => (n, i)))
            {
                command = parentCommand.Subcommands.FirstOrDefault(c => string.Compare(subName, c.Name, StringComparison.Ordinal) == 0 || c.Aliases.Contains(subName));
                if (command == null)
                {
                    bool isLast = (i == subcommandNames.Length - 1);
                    command = new Command(subName, isLast ? desc.Description : null);
                    created = true;
                    parentCommand.Add(command);
                }
                parentCommand = command;
            }
            if (command == null) // this is to satisfy compiler, in fact command cannot be null here because we already checked that name is not empty and we either found existing or created new command
                throw new InvalidOperationException();
            if (!created)
                throw new AttributeUsageException($"Command '{name}' has multiple [Command] definitions");
            foreach (var alias in SplitNames(attr.Aliases))
                command.Aliases.Add(alias);
            command.Hidden = attr.Hidden;

            if (desc.Method != null)
            {
                AddCommandHandler(command, desc.Method, desc.MutuallyExclusiveOptionsArguments);
                // GetBinding() is documented to return the declaring entity for any command, not just
                // the root one, so record the handler method here as well.
                _bindings[command] = desc.Method;
            }

            if (desc.RecursiveOptionsContainingType != null)
                foreach (var opt in recursiveOptions.Where(x => x.ContainingType == desc.RecursiveOptionsContainingType))
                {
                    opt.CliOption.Recursive = true;
                    command.Add(opt.CliOption);
                }

            return command;
        }

        internal static readonly char[] NameListDelimiters = " ,;".ToCharArray();
        internal static IEnumerable<string> SplitNames(string? names)
        {
            if (names == null)
                return Array.Empty<string>();
            return names.Split(NameListDelimiters, StringSplitOptions.RemoveEmptyEntries)
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .ToArray();
        }

        private static readonly Type[] SupportedReturnTypes = new[] { typeof(void), typeof(int), typeof(Task<int>), typeof(Task)
            , typeof(ValueTask<int>), typeof(ValueTask)
        };

        private static void AddCommandHandler(Command command, MethodInfo method, string? mutuallyExclusiveOptionsArguments)
        {
            if (command.Action != null)
                throw new AttributeUsageException($"Command '{command.Name}' has multiple handler methods");

            if (!method.IsStatic)
                throw new AttributeUsageException($"Method {method.Name} declared as [Command] must be static");

            if (!SupportedReturnTypes.Any(t => t.IsAssignableFrom(method.ReturnType)))
                throw new AttributeUsageException($"Method {method.Name} return type must be one of {string.Join(", ", SupportedReturnTypes.Select(t => GetFullTypeName(t)))}.");

            var paramInfo = new List<Symbol>();

            foreach (var param in method.GetParameters())
            {
                var opt = GetCustomAttribute<ParameterInfo, OptionAttribute>(param);
                var arg = GetCustomAttribute<ParameterInfo, ArgumentAttribute>(param);
                if (opt != null && arg != null)
                    throw new AttributeUsageException($"Parameter {param.Name} of method {method.Name} declared as both [Option] and [Argument]");

                Func<object?>? getDefaultValue = null;
                if (param.HasDefaultValue)
                    getDefaultValue = () => param.DefaultValue;

                if (arg != null)
                {
                    var argument = CreateArgument(arg, param.Name, param.ParameterType, getDefaultValue);
                    command.Add(argument);
                    paramInfo.Add(argument);
                    _bindings.Add(argument, param);
                }
                else
                {
                    var option = CreateOption(opt ?? new OptionAttribute(), param.Name, param.ParameterType, getDefaultValue);
                    command.Add(option);
                    paramInfo.Add(option);
                    _bindings.Add(option, param);
                }
            }

            command.SetAction(async (parseResult, cancellationToken) =>
            {
                ParseResult = parseResult;
                CancellationToken = cancellationToken;

                int exitCode = 0;
                Exception? exceptionToRethrow = null;
                try
                {
                    // Binding global/recursive options runs user code (property setters), and reading
                    // parameter values can evaluate default value factories, so both must be covered
                    // by the exception handler.
                    foreach (var opt in recursiveOptions)
                        opt.SetValueFromCommandLine();

                    var methodParams = paramInfo.Select(param =>
                    {
                        switch (param)
                        {
                            case Option opt:
                                return GetSymbolValue(parseResult, opt);
                            case Argument arg:
                                return GetSymbolValue(parseResult, arg);
                            default:
                                throw new InvalidOperationException();
                        }
                    }).ToArray();

                    if (mutuallyExclusiveOptionsArguments != null)
                        ParseResult.ValidateMutuallyExclusiveOptionsArguments(mutuallyExclusiveOptionsArguments);

                    var beforeCommandEventArguments = new BeforeCommandEventArguments(parseResult);
                    BeforeCommand?.Invoke(beforeCommandEventArguments);

                    var awaitable = method.Invoke(null, methodParams);

                    if (awaitable == null)
                    {
                        exitCode = 0;
                    }
                    else
                    {
                        switch (awaitable)
                        {
                            case Task<int> t:
                                exitCode = await t;
                                break;
                            case Task t:
                                await t;
                                exitCode = 0;
                                break;
                            case ValueTask<int> t:
                                exitCode = await t;
                                break;
                            case ValueTask t:
                                await t;
                                exitCode = 0;
                                break;
                            case int i:
                                exitCode = i;
                                break;
                            default:
                                // should not be here because of SupportedReturnTypes check above
                                throw new InvalidOperationException();
                        }
                    }
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    (exitCode, exceptionToRethrow) = InvokeExceptionHandler(ex.InnerException);
                }
                catch (Exception ex)
                {
                    (exitCode, exceptionToRethrow) = InvokeExceptionHandler(ex);
                }

                // AfterCommand fires unconditionally, even when an exception occurred
                var afterCommandEventArguments = new AfterCommandEventArguments(parseResult, exitCode);
                AfterCommand?.Invoke(afterCommandEventArguments);
                exitCode = afterCommandEventArguments.ExitCode;

                if (exceptionToRethrow != null)
                    ExceptionDispatchInfo.Capture(exceptionToRethrow).Throw();

                return exitCode;
            });
        }

        private static object? GetSymbolValue(ParseResult parseResult, Symbol symbol)
        {
            var result = parseResult.GetResult(symbol);
            if (result == null)
            {
                switch (symbol)
                {
                    case Option opt: return opt.GetDefaultValue();
                    case Argument arg: return arg.GetDefaultValue();
                    default: return null;
                }
            }

            var valueType = symbol switch
            {
                Option opt => opt.ValueType,
                Argument arg => arg.ValueType,
                _ => typeof(object)
            };

            return GetValueOrDefault(result, valueType);
        }

        // OptionResult/ArgumentResult only expose the strongly typed GetValueOrDefault<T>(), while the
        // value type is known only at runtime, so the call is bound reflectively. Resolved methods are
        // cached because this runs for every parameter of every invocation.
        private static readonly Dictionary<(Type resultType, Type valueType), MethodInfo> _getValueMethods
            = new Dictionary<(Type, Type), MethodInfo>();

        private static object? GetValueOrDefault(SymbolResult result, Type valueType)
        {
            var key = (result.GetType(), valueType);
            MethodInfo? method;
            lock (_getValueMethods)
            {
                if (!_getValueMethods.TryGetValue(key, out method))
                {
                    var definition = key.Item1.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(m => m.Name == "GetValueOrDefault"
                                          && m.IsGenericMethodDefinition
                                          && m.GetGenericArguments().Length == 1
                                          && m.GetParameters().Length == 0)
                        ?? throw new MissingMethodException(key.Item1.FullName, "GetValueOrDefault");
                    method = definition.MakeGenericMethod(valueType);
                    _getValueMethods[key] = method;
                }
            }
            return method.Invoke(result, null);
        }

        private static string GetFullTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                string genericArguments = type.GetGenericArguments()
                                    .Select(x => x.Name)
                                    .Aggregate((x1, x2) => $"{x1}, {x2}");
                return $"{type.Name.Substring(0, type.Name.IndexOf("`"))}"
                     + $"<{genericArguments}>";
            }
            return type.Name;
        }

        private static Option CreateOption(OptionAttribute info, string? memberName, Type valueType, Func<object?>? getDefaultValue = null)
        {
            var genericType = typeof(Option<>).MakeGenericType(new[] { valueType });
            var name = info.Name ?? memberName?.ToKebabCase() ?? throw new NotSupportedException($"Option name cannot be deduced from parameter [{info}], specify name explicitly");
            name = AddPrefix(name);
            Option instance = (Option)Activator.CreateInstance(genericType, new object[] { name, Array.Empty<string>() })!;
            if (info.Description != null)
                instance.Description = info.Description;
            if (info.Arity.HasValue)
                instance.Arity = info.Arity.Value;
            if (info.HelpName != null)
                instance.HelpName = info.HelpName;
            instance.Hidden = info.Hidden;
            foreach (var alias in SplitNames(info.Aliases))
                instance.Aliases.Add(AddPrefix(alias));
            if (info.Required == false && getDefaultValue != null)
                SetDefaultValueFactory(instance, valueType, getDefaultValue);
            else
                instance.Required = true;
            return instance;

            static string AddPrefix(string name)
            {
                if (name.StartsWith("-"))
                    return name;
                if (name.Length == 1)
                    return "-" + name;
                return "--" + name;
            }
        }

        private static Argument CreateArgument(ArgumentAttribute info, string? memberName, Type valueType, Func<object?>? getDefaultValue = null)
        {
            var genericType = typeof(Argument<>).MakeGenericType(new[] { valueType });
            var name = info.Name ?? memberName?.ToKebabCase() ?? throw new NotSupportedException($"Argument name cannot be deduced from parameter [{info}], specify name explicitly");
            Argument instance = (Argument)Activator.CreateInstance(genericType, new object[] { name })!;
            if (info.Description != null)
                instance.Description = info.Description;
            if (info.Arity.HasValue)
                instance.Arity = info.Arity.Value;
            if (info.HelpName != null)
                instance.HelpName = info.HelpName;
            instance.Hidden = info.Hidden;
            if (getDefaultValue != null)
                SetDefaultValueFactory(instance, valueType, getDefaultValue);
            return instance;
        }

        private static void SetDefaultValueFactory(object instance, Type valueType, Func<object?> getDefaultValue)
        {
            var factory = typeof(CLI)
                .GetMethod(nameof(CreateTypedDefaultValueFactory), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(valueType)
                .Invoke(null, new object[] { getDefaultValue });
            instance.GetType().GetProperty("DefaultValueFactory")!.SetValue(instance, factory);
        }

        private static Func<ArgumentResult, T> CreateTypedDefaultValueFactory<T>(Func<object?> getDefaultValue)
        {
            return _ => (T)getDefaultValue()!;
        }

        private static string ToKebabCase(this string str)
        {
            return Regex.Replace(str, @"([a-z])([A-Z][a-z])", "$1-$2").ToLower();
        }
    }
}

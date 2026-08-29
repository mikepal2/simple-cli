using SnapCLI;
using System.CommandLine;
using System.Reflection;

[assembly: Command(Name = "level1cmd")]

namespace Tests
{

    [TestClass]
    public class UnitTest1 : SnapCliUnitTest
    {
        [TestMethod]
        [DataRow("", "[testhost()]")]
        [DataRow("-?", "Root description")]
        [DataRow("test1", "like:[before:test1]*[test1()]*[after:test1]")]
        [DataRow("test2", "'-p' is required")]
        [DataRow("test2 -p 1", "test2(1)")]
        [DataRow("test3", "test3(2)")]
        [DataRow("test3 --param 3", "test3(3)")]
        [DataRow("test4", "test4()")]
        [DataRow("test4 --param foo", "test4(foo)")]
        [DataRow("test-field", "test-field(globalOptionFieldDefaultValue)")]
        [DataRow("test-field --global-option-field 1 ", "test-field(1)")]
        [DataRow("test-property", "test-property(globalOptionPropertyDefaultValue)")]
        [DataRow("test-property --prop 123 ", "test-property(123)")]
        [DataRow("test6", "Required argument missing")]
        [DataRow("test6 --opt1name", "Required argument")]
        [DataRow("test6 --opt1name a", "expected type 'System.Int32'")]
        [DataRow("test6 --opt1name 222", "test6(True,222,1,arg2)")]
        [DataRow("test6 --opt1alias 222", "test6(True,222,1,arg2)")]
        [DataRow("test6 -O 222", "test6(True,222,1,arg2)")]
        [DataRow("test6 --opt1name false 222", "test6(False,222,1,arg2)")]
        [DataRow("test6 -O false 222", "test6(False,222,1,arg2)")]
        [DataRow("test6 --opt1name false --option2", "Required argument missing")]
        [DataRow("test6 --opt1name false 222 --option2 2", "test6(False,222,2,arg2)")]
        [DataRow("test6 --opt1name false 222 --option2 2 arg2_new", "test6(False,222,2,arg2_new)")]
        [DataRow("test6 --opt1name --option2 2 222 arg2_new", "test6(True,222,2,arg2_new)")] // change order - options first
        [DataRow("test6 222 arg2_new --opt1name false --option2 2 ", "test6(False,222,2,arg2_new)")] // change order - arguments first
        [DataRow("test6 -?", "like:Test6 description*test6 <arg1name> [<argument2>] [options]")]
        [DataRow("test6 -?", "<arg1help>  arg1 description")]
        [DataRow("test6 -?", "<arg2help>  arg2 description [default: arg2]")]
        [DataRow("test6 -?", "like: -O, --opt1alias, --opt1name*(REQUIRED)*opt1 description")]
        [DataRow("test6 -?", "like:--option2 <opt2help>*opt2 description [default: 1]")]
        [DataRow("test6 -?", "--global-option-field")]
        [DataRow("test6 -?", "[default: globalOptionFieldDefaultValue]")]
        [DataRow("test6 -?", "--prop, --propAlias <propHelpName>")]
        [DataRow("test6 -?", "Prop description [default: globalOptionPropertyDefaultValue]")]
        [DataRow("exception", $"like:[exception()]*\nSystem.ApplicationException: THIS IS TEST-GENERATED EXCEPTION!*[exitCode:1]", UseExceptionHandler.Default)]
        [DataRow("exception", "like:[exception:THIS IS TEST-GENERATED EXCEPTION!]*[exitCode:999]", UseExceptionHandler.Custom)]
        [DataRow("exception", "like:[unhandled:System.ApplicationException: THIS IS TEST-GENERATED EXCEPTION!", UseExceptionHandler.Null)]
        [DataRow("exception", "like:[throw:System.ApplicationException: THIS IS TEST-GENERATED EXCEPTION!", UseExceptionHandler.Throwing)]
        [DataRow("exception", "like:[unhandled:System.Exception: Rethrowing", UseExceptionHandler.Throwing)]
        [DataRow("exit-code", "[exitCode:0]")]
        [DataRow("exit-code --exit-code 1", "[exitCode:1]")]
        [DataRow("exit-code --exit-code -1", "[exitCode:-1]")]
        [DataRow("exit-code-async", "[exitCode:0]")]
        [DataRow("exit-code-async --exit-code 1", "[exitCode:1]")]
        [DataRow("exit-code-async --exit-code -1", "[exitCode:-1]")]
        [DataRow("level1cmd -?", "regex:Commands:\\s*level2cmd")]
        [DataRow("level1cmd level2cmd", "[level1cmd_level2cmd()]")]
        [DataRow("validate-mutually-exclusive-options", "!mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options --opt1 1 --opt2 2", "mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options --opt1 1 --propAlias 2", "mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options --opt2 1 --propAlias 2", "!mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options --opt2 1 --global-option-field 2", "mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options2 --opt1 1 --opt2 2", "mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options2 --opt1 1 --propAlias 2", "mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options2 --opt2 1 --propAlias 2", "!mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options2 --opt2 1 --global-option-field 2", "mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options2 3 --opt2 1", "mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options2 3 --opt1 1", "!mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options3 --opt1 1 --opt2 2", "mutually exclusive")]
        [DataRow("validate-mutually-exclusive-options3 --opt2 1 3", "mutually exclusive")]
        [DataRow("cmd-r1", "[cmd-r1(11,1,opt2,opt3)]")]
        [DataRow("cmd-r1 --r-opt1 111", "[cmd-r1(11,111,opt2,opt3)]")]
        [DataRow("cmd-r1 --r-opt1 111 --r-opt2 opt222 --r-opt3 opt333", "[cmd-r1(11,111,opt222,opt333)]")]
        [DataRow("cmd-r1 --r-opt1 1111 --r-opt2 opt222s --r-opt3 opt333s sub-cmd-r1 --opt1 101", "[cmd-r1_sub-cmd-r1(101,1111,opt222s,opt333s)]")]
        [DataRow("cmd-r2 --opt1 11 --r-opt1 111 --r-opt2 opt222 --r-opt3 opt333", "[cmd-r2(11,1,opt2,opt3,111,opt222,opt333)]")]

        // a required option is reported even when nothing else is missing
        [DataRow("require-option", "'--needed' is required")]
        [DataRow("require-option --needed x", "[require-option(x)]")]

        // return types that were previously untested
        [DataRow("value-task-int", "[exitCode:0]")]
        [DataRow("value-task-int --exit-code 5", "[exitCode:5]")]
        [DataRow("value-task-void", "[value-task-void()]")]
        [DataRow("value-task-void", "[exitCode:0]")]

        // hidden commands run but do not appear in help
        [DataRow("hidden-command", "[hidden-command()]")]
        [DataRow("-?", "!hidden-command")]

        // command aliases
        [DataRow("TEST6 --opt1name 222", "test6(True,222,1,arg2)")]

        // arity
        [DataRow("arity 1", "[arity(1)]")]
        [DataRow("arity 1 2", "[arity(1,2)]")]
        [DataRow("arity 1 2 3", "Unrecognized command or argument '3'")]

        // a global option setter must not run when the option is absent from the command line
        [DataRow("watch-setter", "!setter-invoked")]
        [DataRow("watch-setter --watched x", "setter-invoked:x")]

        // an exception thrown while binding a global option is routed to the exception handler
        [DataRow("watch-setter --watched throw", "like:*[exception:setter rejected 'throw']*[exitCode:999]", UseExceptionHandler.Custom)]

        // mutually exclusive violations are reported as input errors, not as a stack trace
        [DataRow("validate-mutually-exclusive-options --opt1 1 --opt2 2", "!   at SnapCLI")]

        // a name that matches no option/argument is rejected instead of silently passing
        [DataRow("mutually-exclusive-bad-name", "'no-such-option' listed as mutually exclusive")]
        public void Test(string commandLine, string pattern, UseExceptionHandler useExceptionHandler = UseExceptionHandler.Default)
        {
            TestCLI(commandLine, pattern, useExceptionHandler);
        }

        [TestMethod]
        public async Task RunAsyncInvokesHandler()
        {
            var output = new StringWriter();
            int exitCode = await CLI.RunAsync(["exit-code-async", "--exit-code", "7"], output, output);
            Assert.AreEqual(7, exitCode);
        }

        [TestMethod]
        public void GetBindingReturnsDeclaringMember()
        {
            var command = CLI.RootCommand.GetCommand("test6");
            Assert.IsNotNull(command);
            Assert.AreEqual(nameof(Test6Handler), (CLI.GetBinding(command) as MethodInfo)?.Name);

            var option = command.GetOption("opt1name");
            Assert.IsNotNull(option);
            Assert.AreEqual("option1", (CLI.GetBinding(option) as ParameterInfo)?.Name);
            Assert.IsNotNull(CLI.GetBindingCustomAttributeProvider(option));
        }

        [TestMethod]
        public void GetOptionMatchesNameAndAlias()
        {
            var command = CLI.RootCommand.GetCommand("test6");
            Assert.IsNotNull(command);
            Assert.IsNotNull(command.GetOption("opt1name"));
            Assert.IsNotNull(command.GetOption("--opt1name"));
            Assert.IsNotNull(command.GetOption("opt1alias"));
            Assert.IsNotNull(command.GetOption("O"));
            Assert.IsNull(command.GetOption("no-such-option"));
        }

        [TestMethod]
        public void GetCommandResolvesSubcommandsAndAliases()
        {
            Assert.IsNotNull(CLI.RootCommand.GetCommand("level1cmd level2cmd"));
            Assert.IsNotNull(CLI.RootCommand.GetCommand("test6"));
            Assert.IsNotNull(CLI.RootCommand.GetCommand("TEST6"));
            Assert.IsNull(CLI.RootCommand.GetCommand("no-such-command"));
            Assert.AreEqual("cmd-r1 sub-cmd-r1", CLI.RootCommand.GetCommand("cmd-r1 sub-cmd-r1")!.FullName());
        }

        [TestMethod]
        [DataRow("(opt1,opt2", "unmatched parentheses")]      // missing closing parenthesis
        [DataRow("(opt1,opt2))", "unmatched parentheses")]    // stray closing parenthesis
        [DataRow("opt1,no-such-option", "is not an option or argument")] // typo must not pass silently
        public void InvalidMutuallyExclusiveSpecIsRejected(string spec, string expected)
        {
            var parseResult = CLI.RootCommand.Parse("validate-mutually-exclusive-options");
            var ex = Assert.ThrowsExactly<AttributeUsageException>(
                () => parseResult.ValidateMutuallyExclusiveOptionsArguments(spec));
            StringAssert.Contains(ex.Message, expected);
        }

        [Startup]
        public static void Startup(InvocationConfiguration config)
        {
            Assert.IsNotNull(config);
            // InvocationConfiguration can be used to configure output, error, timeout, etc.
            // SnapCLI already sets EnableDefaultExceptionHandler = false by default.
        }

        [Startup]
        public static void Stratup2()
        {
            CLI.BeforeCommand += (args) => { Out.WriteLine($"[before:{args.ParseResult.CommandResult.Command.Name}]"); };
            CLI.AfterCommand += (args) => { Out.WriteLine($"[after:{args.ParseResult.CommandResult.Command.Name}]"); };

            CLI.ExceptionHandler = (exception) =>
            {
                // 
                if (exception is not OperationCanceledException)
                    Out.WriteLine(exception);

                return 1; // exit code
            };
        }

        [Option] // implicit Name= global-option-field
        public static string? globalOptionField = globalOptionFieldDefaultValue;
        private const string globalOptionFieldDefaultValue = "globalOptionFieldDefaultValue";

        [Option(Name = "prop", HelpName = "propHelpName", Aliases = "propAlias", Description = "Prop description")]
        public static string? globalOptionProperty { get; set; } = globalOptionPropertyDefaultValue;
        private const string globalOptionPropertyDefaultValue = "globalOptionPropertyDefaultValue";

        [Command]
        public static void Test1()
        {
            TraceCommand();
        }


        [Command]
        public static void Test2(int p)
        {
            TraceCommand(p);
        }

        [Command]
        public static void Test3(int param = 2)
        {
            TraceCommand(param);
        }

        [Command]
        public static void Test4(string? param = null)
        {
            TraceCommand(param);
        }

        [Command]
        public static void TestField()
        {
            TraceCommand(globalOptionField);
        }

        [Command]
        public static void TestProperty()
        {
            TraceCommand(globalOptionProperty);
        }

        [RootCommand(Description = "Root description")]
        public static void TestRoot()
        {
            TraceCommand();
        }

        [Command(Name = "test6", Aliases = "TEST6", Description = "Test6 description")]
        public static void Test6Handler(
            [Option(Name = "opt1name", HelpName = "opt1help", Aliases = "opt1alias,O", Description = "opt1 description")]
            bool option1,

            [Argument(Name = "arg1name", HelpName = "arg1help", Description = "arg1 description")]
            int argument1,


            [Option(HelpName= "opt2help", Description= "opt2 description")]
            int option2 = 1,

            [Argument(HelpName= "arg2help", Description= "arg2 description")]
            string argument2 = "arg2"
            )
        {
            TraceCommand(option1, argument1, option2, argument2);
        }

        [Command]
        public static void exception()
        {
            TraceCommand();
            throw new ApplicationException("THIS IS TEST-GENERATED EXCEPTION!");
        }

        [Command]
        public static int exitCode(int exitCode = 0)
        {
            return exitCode;
        }

        [Command]
        public static Task<int> exitCodeAsync(int exitCode = 0)
        {
            return Task.FromResult(exitCode);
        }

        [Command(Name = "level1cmd level2cmd")]
        public static void level2cmd()
        {
            TraceCommand();
        }

        [Command]
        public static void ValidateMutuallyExclusiveOptions(
            int opt1 = 1, int opt2 = 2)
        {
            TraceCommand();
            CLI.ParseResult.ValidateMutuallyExclusiveOptionsArguments(["opt1", "opt2"]);
            CLI.ParseResult.ValidateMutuallyExclusiveOptionsArguments(["opt1", "prop"]);
            CLI.ParseResult.ValidateMutuallyExclusiveOptionsArguments(["opt2", "global-option-field"], ["validate-mutually-exclusive-options"]);
        }

        [Command(MutuallyExclusiveOptionsArguments = "(opt1,opt2)(opt1,prop)(opt2,global-option-field)(opt2,arg1)")]
        public static void ValidateMutuallyExclusiveOptions2(
            int opt1 = 1, int opt2 = 2, [Argument] int arg1 = 3)
        {
            TraceCommand();
        }

        [Command(MutuallyExclusiveOptionsArguments = "opt1,opt2,arg1")]
        public static void ValidateMutuallyExclusiveOptions3(int opt1 = 1, int opt2 = 2, [Argument] int arg1 = 3)
        {
            TraceCommand();
        }

        class RecursiveOptions1
        {
            [Option] internal static int rOpt1 = 1;
            [Option] internal static string rOpt2 = "opt2";
            [Option] internal static string rOpt3 { get; set; } = "opt3";
        }

        [Command(Name = "cmd-r1", RecursiveOptionsContainingType = typeof(RecursiveOptions1))]
        public static void TestRecursiveOptions(int opt1 = 11)
        { 
            TraceCommand(
                opt1,
                RecursiveOptions1.rOpt1, 
                RecursiveOptions1.rOpt2,
                RecursiveOptions1.rOpt3
                );
        }

        [Command(Name = "cmd-r1 sub-cmd-r1")]
        public static void TestRecursiveOptionsSubcommand(int opt1 = 0)
        {
            TraceCommand(
                opt1,
                // this recursive options must be available from parent command
                RecursiveOptions1.rOpt1,
                RecursiveOptions1.rOpt2,
                RecursiveOptions1.rOpt3
                );
        }

        class RecursiveOptions2
        {
            [Option] internal static int rOpt1 = 11;
            [Option] internal static string rOpt2 = "opt22";
            [Option] internal static string rOpt3 { get; set; } = "opt33";
        }

        [Command(Name = "cmd-r2", RecursiveOptionsContainingType = typeof(RecursiveOptions2))]
        public static void TestRecursiveOptions2(int opt1 = 0)
        {
            TraceCommand(
                opt1,
                // this is invalid access, values will stay default
                RecursiveOptions1.rOpt1,
                RecursiveOptions1.rOpt2,
                RecursiveOptions1.rOpt3,
                // this is valid access, values must be reflecyed
                RecursiveOptions2.rOpt1,
                RecursiveOptions2.rOpt2,
                RecursiveOptions2.rOpt3
                );
        }

        [Command(Name = "require-option")]
        public static void RequireOption(
            [Option(Name = "needed", Required = true)] string needed = "unused-default")
        {
            TraceCommand(needed);
        }

        [Command(Name = "value-task-int")]
        public static ValueTask<int> ValueTaskInt(int exitCode = 0) => new ValueTask<int>(exitCode);

        [Command(Name = "value-task-void")]
        public static ValueTask ValueTaskVoid()
        {
            TraceCommand();
            return default;
        }

        [Command(Name = "hidden-command", Hidden = true)]
        public static void HiddenCommand() => TraceCommand();

        [Command(Name = "arity")]
        public static void Arity([Argument(1, 2, Name = "numbers")] int[] numbers)
            => Out.WriteLine($"[arity({string.Join(",", numbers)})]");

        // Global option whose setter reports every invocation, so a test can assert that the setter
        // does not run when the option is absent from the command line.
        [Option(Name = "watched")]
        public static string? watchedOption
        {
            get => _watchedOption;
            set
            {
                Out.WriteLine($"[setter-invoked:{value}]");
                if (value == "throw")
                    throw new InvalidOperationException("setter rejected 'throw'");
                _watchedOption = value;
            }
        }
        private static string? _watchedOption = "watched-default";

        [Command(Name = "watch-setter")]
        public static void WatchSetter() => TraceCommand(watchedOption);

        [Command(Name = "mutually-exclusive-bad-name")]
        public static void MutuallyExclusiveBadName(int opt1 = 1)
        {
            CLI.ParseResult.ValidateMutuallyExclusiveOptionsArguments(["opt1", "no-such-option"]);
        }
    }


}

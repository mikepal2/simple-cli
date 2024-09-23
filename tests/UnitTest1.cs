using SnapCLI;
using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;
namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        static StringWriter Out = new StringWriter();

        [TestMethod]
        [DataRow("", "[testhost()]")]
        [DataRow("-?", "Root description")]
        [DataRow("test1", "like:*[before:test1]*[test1()]*[after:test1]*")]
        [DataRow("test2", "'-p' is required")]
        [DataRow("test2 -p 1", "test2(1)")]
        [DataRow("test3", "test3(2)")]
        [DataRow("test3 --param 3", "test3(3)")]
        [DataRow("test4", "test4()")]
        [DataRow("test4 --param foo", "test4(foo)")]
        [DataRow("testfield", "testfield(globalOptionFieldDefaultValue)")]
        [DataRow("testfield --globalOptionField 1 ", "testfield(1)")]
        [DataRow("testproperty", "testproperty(globalOptionPropertyDefaultValue)")]
        [DataRow("testproperty --prop 123 ", "testproperty(123)")]
        [DataRow("test6", "'--opt1name' is required")]
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
        [DataRow("test6 -?", "like:*Test6 description*test6 <arg1name> [<argument2>] [options]*")]
        [DataRow("test6 -?", "<arg1help>  arg1 description")]
        [DataRow("test6 -?", "<arg2help>  arg2 description [default: arg2]")]
        [DataRow("test6 -?", "like:* -O, --opt1alias, --opt1name *(REQUIRED)*opt1 description*")]
        [DataRow("test6 -?", "like:*--option2 <opt2help>*opt2 description [default: 1]*")]
        [DataRow("test6 -?", "--globalOptionField")]
        [DataRow("test6 -?", "[default: globalOptionFieldDefaultValue]")]
        [DataRow("test6 -?", "--prop, --propAlias <propHelpName>")]
        [DataRow("test6 -?", "Prop description [default: globalOptionPropertyDefaultValue]")]
        [DataRow("exception", "like:*[exception()]*TEST-GENERATED EXCEPTION*")]
        [DataRow("exception", "![exitCode:0]")]
        [DataRow("exitcode", "[exitCode:0]")]
        [DataRow("exitcode --exitCode 1", "[exitCode:1]")]
        [DataRow("exitcode --exitCode -1", "[exitCode:-1]")]
        [DataRow("exitcodeasync", "[exitCode:0]")]
        [DataRow("exitcodeasync --exitCode 1", "[exitCode:1]")]
        [DataRow("exitcodeasync --exitCode -1", "[exitCode:-1]")]
        public void TestCLI(string commandLine, string pattern)
        {
            lock (Out)
            {
                Out.GetStringBuilder().Clear();
                int exitCode = CLI.Run(SplitArgs(commandLine).ToArray());
                Out.WriteLine($"[exitCode:{exitCode}]");
                var output = Out.ToString().Trim(" \r\n".ToCharArray());
                bool expectedContains = true;

                if (pattern.StartsWith("!"))
                {
                    expectedContains = false;
                    pattern = pattern.Substring(1);
                }
                bool contains;
                if (pattern.StartsWith("like:"))
                    contains = output.Like(pattern.Substring("like:".Length));
                else if (pattern.StartsWith("regex:"))
                    contains = Regex.IsMatch(output, pattern.Substring("regex:".Length));
                else
                    contains = output.Contains(pattern);

                Assert.IsTrue(contains == expectedContains, $"Output {(expectedContains ? "does not contain" : "contains")} '{pattern}'\n\nOutput:\n{Out}\n");
            }
        }

        private static void TraceCommand(params object?[] args)
        {
            Out.WriteLine($"[{CLI.CurrentCommand.Name}({string.Join(",", args)})]");
        }

        [Startup]
        public static void Startup(CliConfiguration configuration)
        {
            Assert.IsNotNull(configuration);
            configuration.Output = Out;
            configuration.Error = Out;
        }

        [Startup]
        public static void Stratup2()
        {
            CLI.BeforeCommand += (parseResult, command) => { Out.WriteLine($"[before:{command.Name}]"); };
            CLI.AfterCommand += (parseResult, command) => { Out.WriteLine($"[after:{command.Name}]"); };
        }

#if BEFORE_AFTER_COMMAND_ATTRIBUTE
        [BeforeCommand]
        public static void PreCommand(ParseResult parseResult, Command command)
        {
            Out.WriteLine($"[PreCommand:{command.Name}]");
        }


        [AfterCommand]
        public static void PostCommand(ParseResult parseResult, Command command)
        {
            Out.WriteLine($"[PostCommand:{command.Name}]");
        }
#endif

        [Option]
        public static string? globalOptionField = globalOptionFieldDefaultValue;
        private const string globalOptionFieldDefaultValue = "globalOptionFieldDefaultValue";

        [Option(name: "prop", helpName: "propHelpName", aliases: ["propAlias"], description: "Prop description")]
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
            globalOptionField = globalOptionFieldDefaultValue; // restore default for other tests
        }

        [Command]
        public static void TestProperty()
        {
            TraceCommand(globalOptionProperty);
            globalOptionProperty = globalOptionPropertyDefaultValue; // restore default for other tests
        }

        [RootCommand("Root description")]
        public static void TestRoot()
        {
            TraceCommand();
        }

        [Command(name: "test6", aliases: ["TEST6"], description: "Test6 description")]
        public static void Test6Handler(
            [Option(name:"opt1name", helpName:"opt1help", aliases:["opt1alias", "O"], description:"opt1 description")]
            bool option1,

            [Argument(name:"arg1name", helpName:"arg1help", description:"arg1 description")]
            int argument1,


            [Option(helpName: "opt2help", description: "opt2 description")]
            int option2 = 1,

            [Argument(helpName: "arg2help", description: "arg2 description")]
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


        // https://stackoverflow.com/a/64236441
        public static IEnumerable<string> SplitArgs(string commandLine)
        {
            var result = new StringBuilder();

            var quoted = false;
            var escaped = false;
            var started = false;
            var allowcaret = false;
            for (int i = 0; i < commandLine.Length; i++)
            {
                var chr = commandLine[i];

                if (chr == '^' && !quoted)
                {
                    if (allowcaret)
                    {
                        result.Append(chr);
                        started = true;
                        escaped = false;
                        allowcaret = false;
                    }
                    else if (i + 1 < commandLine.Length && commandLine[i + 1] == '^')
                    {
                        allowcaret = true;
                    }
                    else if (i + 1 == commandLine.Length)
                    {
                        result.Append(chr);
                        started = true;
                        escaped = false;
                    }
                }
                else if (escaped)
                {
                    result.Append(chr);
                    started = true;
                    escaped = false;
                }
                else if (chr == '"')
                {
                    quoted = !quoted;
                    started = true;
                }
                else if (chr == '\\' && i + 1 < commandLine.Length && commandLine[i + 1] == '"')
                {
                    escaped = true;
                }
                else if (chr == ' ' && !quoted)
                {
                    if (started) yield return result.ToString();
                    result.Clear();
                    started = false;
                }
                else
                {
                    result.Append(chr);
                    started = true;
                }
            }

            if (started) yield return result.ToString();
        }
    }

}

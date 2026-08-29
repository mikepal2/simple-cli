using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnapCLI;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests
{
    public class SnapCliUnitTest : StaticFieldsCache
    {
        public static StringWriter Out = new StringWriter();

        public enum UseExceptionHandler
        {
            Default,
            Custom,
            Null,
            Throwing
        }
        public void TestCLI(string commandLine, string pattern, UseExceptionHandler useExceptionHandler = UseExceptionHandler.Default)
        {
            // synchronize tests
            lock (Out)
            {
                var defaultExceptionHandler = CLI.ExceptionHandler;
                try
                {

                    ResetValuesFromCache();

                    // Restoring cached values invokes property setters, which may write to Out.
                    // Discard that so the assertions only see output produced by the run itself.
                    Out.GetStringBuilder().Clear();

                    switch (useExceptionHandler)
                    {
                        case UseExceptionHandler.Default: break;
                        case UseExceptionHandler.Custom: CLI.ExceptionHandler = CustomExceptionHandler; break;
                        case UseExceptionHandler.Null: CLI.ExceptionHandler = null; break;
                        case UseExceptionHandler.Throwing: CLI.ExceptionHandler = ThrowingExceptionHandler; break;
                        default: throw new ArgumentOutOfRangeException(nameof(useExceptionHandler));
                    }

                    try
                    {
                        int exitCode = CLI.Run(SplitArgs(commandLine).ToArray(), Out, Out);
                        Out.WriteLine($"[exitCode:{exitCode}]");
                    }
                    catch (Exception ex)
                    {
                        Out.WriteLine($"[unhandled:{ex}]");

                    }
                    var output = Out.ToString().Trim(" \r\n".ToCharArray());
                    bool expectedContains = true;

                    if (pattern.StartsWith("!"))
                    {
                        expectedContains = false;
                        pattern = pattern.Substring(1);
                    }
                    bool contains;
                    if (pattern.StartsWith("like:"))
                        contains = output.Like(pattern.Substring("like:".Length), substring: true);
                    else if (pattern.StartsWith("regex:"))
                        contains = Regex.IsMatch(output, pattern.Substring("regex:".Length));
                    else
                        contains = output.Contains(pattern);

                    Assert.IsTrue(contains == expectedContains, $"Output {(expectedContains ? "does not contain" : "contains")} '{pattern}'\n\nOutput:\n{Out}\n");
                }
                finally
                {
                    // cleanup
                    Out.GetStringBuilder().Clear();
                    CLI.ExceptionHandler = defaultExceptionHandler;
                }
            }
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

        public static void TraceCommand(params object?[] args)
        {
            Out.WriteLine($"[{CLI.ParseResult.CommandResult.Command.FullName().Replace(' ', '_')}({string.Join(",", args)})]");
        }

        private static int CustomExceptionHandler(Exception exception)
        {
            Out.WriteLine($"[exception:{exception.Message}]");
            return 999; // exit code
        }

        private static int ThrowingExceptionHandler(Exception exception)
        {
            Out.WriteLine($"[throw:{exception}]");
            throw new Exception("Rethrowing", exception);
        }
    }


}

using System.Diagnostics;
using System.Reflection;

namespace Tests
{
    /// <summary>
    /// Runs the sample applications as child processes and checks what they actually print.
    /// The unit tests exercise the library in-process against one fixed set of attributes; these
    /// tests are what catch a sample that no longer starts at all.
    /// </summary>
    [TestClass]
    public class SampleTests
    {
        // Every project builds to <repo>/bin/<project>/<configuration>/<tfm>/, so the directory that
        // holds this test assembly gives us both the configuration and the tfm.
        private static string BinRoot => Path.GetFullPath(Path.Combine(TestDir, "..", "..", ".."));
        private static string TestDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        private static string Configuration => new DirectoryInfo(TestDir).Parent!.Name;
        private static string TargetFramework => new DirectoryInfo(TestDir).Name;

        private static string AppPath(string project, string assemblyName)
        {
            var path = Path.Combine(BinRoot, project, Configuration, TargetFramework, assemblyName + ".dll");
            Assert.IsTrue(File.Exists(path), $"Application not found: {path}");
            return path;
        }

        private static (int exitCode, string output) Run(string project, string assemblyName, params string[] args)
        {
            var startInfo = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = WorkDir,
            };
            startInfo.ArgumentList.Add(AppPath(project, assemblyName));
            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);

            using var process = Process.Start(startInfo)!;
            var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            Assert.IsTrue(process.WaitForExit(60_000), "Application did not exit within 60 seconds");
            return (process.ExitCode, output);
        }

        private static string WorkDir = null!;

        [ClassInitialize]
        public static void CreateWorkDir(TestContext context)
        {
            WorkDir = Path.Combine(Path.GetTempPath(), "snapcli-sample-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(WorkDir);
        }

        [ClassCleanup]
        public static void DeleteWorkDir()
        {
            try { Directory.Delete(WorkDir, recursive: true); } catch (IOException) { }
        }

        private static void AssertSucceeds(int exitCode, string output)
        {
            Assert.AreEqual(0, exitCode, $"Expected success, got exit code {exitCode}. Output:\n{output}");
            StringAssert.DoesNotMatch(output, new System.Text.RegularExpressions.Regex(@"^\s+at Snap(CLI|Cli)\."),
                $"Output contains a stack trace:\n{output}");
        }

        [TestMethod]
        [DataRow("minimal", "minimal")]
        [DataRow("base64", "base64")]
        [DataRow("quotes", "scl")]
        [DataRow("inventory", "inventory")]
        [DataRow("parameterized-main", "parameterized-main")]
        [DataRow("classic-main", "classic-main")]
        public void SampleShowsHelp(string project, string assemblyName)
        {
            var (exitCode, output) = Run(project, assemblyName, "-?");
            AssertSucceeds(exitCode, output);
            StringAssert.Contains(output, "Show help and usage information");
        }

        [TestMethod]
        public void MinimalGreets()
        {
            AssertSucceeds(Run("minimal", "minimal").exitCode, Run("minimal", "minimal").output);
            StringAssert.Contains(Run("minimal", "minimal").output, "Hello World!");
            StringAssert.Contains(Run("minimal", "minimal", "--name", "Joe").output, "Hello Joe!");
        }

        [TestMethod]
        public void Base64RoundTrips()
        {
            var encoded = Run("base64", "base64", "encode", "Hello World!");
            AssertSucceeds(encoded.exitCode, encoded.output);
            Assert.AreEqual("SGVsbG8gV29ybGQh", encoded.output.Trim());

            var decoded = Run("base64", "base64", "decode", "SGVsbG8gV29ybGQh");
            AssertSucceeds(decoded.exitCode, decoded.output);
            Assert.AreEqual("Hello World!", decoded.output.Trim());
        }

        [TestMethod]
        public void ParameterizedAndClassicMainAcceptArguments()
        {
            foreach (var project in new[] { "parameterized-main", "classic-main" })
            {
                var (exitCode, output) = Run(project, project, "Joe", "--repeat", "2");
                AssertSucceeds(exitCode, output);
                Assert.AreEqual(2, output.Split("Hello Joe!").Length - 1, $"Unexpected output from {project}:\n{output}");
            }
        }

        [TestMethod]
        public void InventoryAddsListsAndRemoves()
        {
            AssertSucceeds(Run("inventory", "inventory", "add", "apple", "3").exitCode, "");

            var list = Run("inventory", "inventory", "list");
            AssertSucceeds(list.exitCode, list.output);
            StringAssert.Contains(list.output, "apple: 3");

            var removed = Run("inventory", "inventory", "remove", "apple", "1");
            AssertSucceeds(removed.exitCode, removed.output);
            StringAssert.Contains(removed.output, "apple: 2");
        }

        [TestMethod]
        public void InventoryReportsMutuallyExclusiveInputAsShortError()
        {
            var (exitCode, output) = Run("inventory", "inventory", "add", "pear", "1");
            AssertSucceeds(exitCode, output);

            var (code, text) = Run("inventory", "inventory", "remove", "--all", "pear", "2");
            Assert.AreEqual(1, code);
            StringAssert.Contains(text, "mutually exclusive");
            StringAssert.DoesNotMatch(text, new System.Text.RegularExpressions.Regex(@"\s+at "),
                $"Input errors must not print a stack trace. Output:\n{text}");
        }

        // The quotes sample declares a top level 'quotes' command, which is why the executable is
        // named 'scl'. Naming it 'quotes' would make every parse fail.
        [TestMethod]
        public void QuotesReadsAndValidatesTheFile()
        {
            var file = Path.Combine(WorkDir, "sampleQuotes.txt");
            File.WriteAllText(file, "A quote." + Environment.NewLine);

            var read = Run("quotes", "scl", "quotes", "read", "--file", file, "--delay", "0");
            AssertSucceeds(read.exitCode, read.output);
            StringAssert.Contains(read.output, "A quote.");

            var missing = Run("quotes", "scl", "quotes", "read", "--file", Path.Combine(WorkDir, "no-such.txt"));
            Assert.AreEqual(1, missing.exitCode);
            StringAssert.Contains(missing.output, "File not found");
            StringAssert.DoesNotMatch(missing.output, new System.Text.RegularExpressions.Regex(@"\s+at "),
                $"Input errors must not print a stack trace. Output:\n{missing.output}");
        }

        [TestMethod]
        [DataRow("shadow-name", "shadow-name", "has the same name as the executable")]
        [DataRow("instance-option", "instance-option", "must be static")]
        public void AttributeMisuseIsReportedAsShortError(string project, string assemblyName, string expected)
        {
            var (exitCode, output) = Run(project, assemblyName, "-?");
            Assert.AreEqual(1, exitCode, $"Expected exit code 1. Output:\n{output}");
            StringAssert.Contains(output, expected);
            StringAssert.Contains(output, "Error: ");
            StringAssert.DoesNotMatch(output, new System.Text.RegularExpressions.Regex(@"Unhandled exception"),
                $"Attribute errors must not crash the process. Output:\n{output}");
        }
    }
}

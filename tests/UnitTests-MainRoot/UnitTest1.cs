using SnapCLI;

namespace Tests
{

    [TestClass]
    public class UnitTestMain : SnapCliUnitTest
    {
        [TestMethod]
        [DataRow("", "Required argument missing")]
        [DataRow("--option1 true", "Required argument missing")]
        [DataRow("--option1 true 2", "[testhost(True,2,1,arg2)]")]
        [DataRow("--option1 true 2 a2", "[testhost(True,2,1,a2)]")]
        [DataRow("--option1 true 2 a2 --option2 3", "[testhost(True,2,3,a2)]")]
        public void Test(string commandLine, string pattern, UseExceptionHandler useExceptionHandler = UseExceptionHandler.Default)
        {
            TestCLI(commandLine, pattern, useExceptionHandler);
        }

        [RootCommand]
        static void Main(
            bool option1,
            [Argument] int argument1,
            int option2 = 1,
            [Argument] string argument2 = "arg2"
            )
        {
            TraceCommand(option1, argument1, option2, argument2);
        }
    }
}

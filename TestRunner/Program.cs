using TestRunner.Models;
using TestRunner.TestExecuter;
using TestRunner.TestLoader;

class Program
{
    private static readonly Dictionary<string, int> PriorityOrder = new()
    {
        ["P0"] = 0,
        ["P1"] = 1,
        ["P2"] = 2,
        ["P3"] = 3
    };
    static void Main(string[] args)
    {
        //get pass to dll
        string? testAssemblyPath;
        if (args.Length > 0)
        {
            testAssemblyPath = args[0];
        }
        else
        {
            Console.Write("Enter path to DLL with tests: ");
            testAssemblyPath = Console.ReadLine();
        }
        if (!File.Exists(testAssemblyPath))
        {
            Console.WriteLine($"Error: File {testAssemblyPath} not found");
            return;
        }

        //load tests
        TestsLoader testLoader = new TestsLoader();
        List<ClassInfo> testClasses = testLoader.LoadTests(testAssemblyPath);

        List<TestResult> results = new List<TestResult>();
        TestExecuter testExecuter = new TestExecuter();

        var sortedClasses = testClasses
            .OrderBy(c => PriorityOrder.GetValueOrDefault(c.Priority ?? "P3", 999));

        foreach (var testClass in sortedClasses)
        {
            int totalTestsInClass = testClass.Tests.Count;
            int executedInClass = 0;

            //group tests by priority
            var testsByPriority = testClass.Tests
                .GroupBy(t => t.Priority)
                .OrderBy(g => PriorityOrder[g.Key]);

            //output test class name and priority
            Console.WriteLine($"Class priority: {testClass.Priority}");
            Console.WriteLine($"Class: {testClass.DisplayName}");

            foreach (var priorityGroup in testsByPriority)
            {
                Console.WriteLine($"\nTests priority: {priorityGroup.Key}");

                //execute tests of the class
                foreach (TestInfo testInfo in priorityGroup)
                {
                    executedInClass++;
                    bool isLastTestInClass = (executedInClass == totalTestsInClass);

                    var result = testExecuter.ExecuteTest(testInfo, isLastTestInClass);
                    results.Add(result);
                    
                    //output test result
                    Console.WriteLine($"{result.Status} {result.TestName} {result.ErrorMessage}");
                }
            }
        }
    }
}
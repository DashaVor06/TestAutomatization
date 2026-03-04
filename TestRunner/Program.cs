using TestRunner.Test;
using TestRunner.TestExecuter;
using TestRunner.TestLoader;

class Program
{
    static async Task Main(string[] args)
    {
        //get pass to dll
        string testAssemblyPath;
        if (args.Length > 0)
        {
            testAssemblyPath = args[0];
        }
        else
        {
            Console.Write("Укажите путь к DLL с тестами: ");
            testAssemblyPath = Console.ReadLine();
        }
        if (!File.Exists(testAssemblyPath))
        {
            Console.WriteLine($"Ошибка: Файл {testAssemblyPath} не найден!");
            return;
        }

        //load tests
        TestsLoader testLoader = new TestsLoader();
        List<TestInfo> tests = testLoader.LoadTests(testAssemblyPath);
        
        //execute tests of each class
        var testsByClass = tests.GroupBy(t => t.ClassType);
        List<TestResult> results = new List<TestResult>();
        TestExecuter testExecuter = new TestExecuter();

        foreach (var classGroup in testsByClass)
        {
            var classTests = classGroup.ToList();
            int testIndexInClass = 1;
            int totalTestsInClass = classTests.Count;

            foreach (TestInfo testInfo in classTests)
            {
                bool isLastTestInClass = (testIndexInClass == totalTestsInClass);

                var result = testExecuter.ExecuteTest(testInfo, isLastTestInClass);
                results.Add(result);

                testIndexInClass++;
            }
        }

        //output results
        foreach (TestResult testResult in results)
        {
            Console.WriteLine($"{testResult.Status} {testResult.TestName} {testResult.ErrorMessage}");
        }
    }
}
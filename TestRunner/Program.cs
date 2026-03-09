using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
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

    private static string GetPathToDll(string[] args)
    {
        string? testAssemblyPath;
        bool fileExists = false;
        do
        {
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
            }
            else
            {
                fileExists = true;
            }
        } while (!fileExists);

        return testAssemblyPath!;
    }

    private static void RunTestsSyncroniosly(List<ClassInfo> testClasses)
    {
        //sort classes by priority
        IEnumerable<ClassInfo> sortedClasses = testClasses
            .OrderBy(c => PriorityOrder.GetValueOrDefault(c.Priority));

        TestExecuter testExecuter = new TestExecuter();

        //start timer
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        foreach (var testClass in sortedClasses)
        {
            if (testClass.Results == null)
                testClass.Results = new ConcurrentBag<TestResult>();
            else
                testClass.Results.Clear();

            int totalTestsInClass = testClass.Tests.Count;
            int executedInClass = 0;

            //group tests by priority
            var testsByPriority = testClass.Tests
                .GroupBy(t => t.Priority)
                .OrderBy(g => PriorityOrder[g.Key]);

            foreach (var priorityGroup in testsByPriority)
            {
                //execute tests of the class
                foreach (TestInfo testInfo in priorityGroup)
                {
                    CancellationToken token = CancellationToken.None;
                    CancellationTokenSource? source = null;

                    if (testInfo.Timeout.HasValue)
                    {
                        source = new CancellationTokenSource(TimeSpan.FromMilliseconds(testInfo.Timeout.Value));
                        token = source.Token;
                    }

                    executedInClass++;
                    bool isLastTestInClass = (executedInClass == totalTestsInClass);

                    TestResult testResult = testExecuter.ExecuteTest(testInfo, token);
                    testClass.Results.Add(testResult);
                }
            }            
        }

        //stop timer
        stopwatch.Stop();
        Console.WriteLine($"Syncroniosly: {stopwatch.ElapsedMilliseconds}ms");
    }

    private static void SetMaxDegreeOfParallelism()
    {
        int processorCount = Environment.ProcessorCount;
        int minWorkerThreads, minCompletionPortThreads;
        ThreadPool.GetMinThreads(out minWorkerThreads, out minCompletionPortThreads);

        int minRecommended = Math.Max(processorCount, minWorkerThreads);
        int maxAllowed = 32767;

        Console.WriteLine($"Current settings:");
        Console.WriteLine($"  CPU cores: {processorCount}");
        Console.WriteLine($"  Min worker threads: {minWorkerThreads}");
        Console.WriteLine($"  Min completion port threads: {minCompletionPortThreads}");
        Console.WriteLine($"  Recommended minimum: {minRecommended}");

        bool validInput = false;
        while (!validInput)
        {
            Console.Write($"\nEnter maximum number of threads: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int maxThreads))
            {
                if (maxThreads < minRecommended)
                {
                    Console.WriteLine($"Error: value cannot be less than {minRecommended}");
                }
                else if (maxThreads > maxAllowed)
                {
                    Console.WriteLine($"Error: value cannot be greater than {maxAllowed}");
                }
                else
                {
                    bool result = ThreadPool.SetMaxThreads(maxThreads, maxThreads);

                    if (result)
                    {
                        validInput = true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to set value. Try another one.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: please enter an integer number");
            }
        }
    }

    private static int GetMaxDegreeOfParallelism()
    {
        bool validInput = false;
        int maxThreads = 1;

        while (!validInput)
        {
            Console.Write($"Enter maximum number of threads: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out maxThreads))
            {
                validInput = true;
            }
            else
            {
                Console.WriteLine("Error: please enter an integer number");
            }
        }
        return maxThreads;
    }

    private static async Task RunTestsInParallel(List<ClassInfo> testClasses)
    {
        int maxParallelTests = GetMaxDegreeOfParallelism();
        using var semaphore = new SemaphoreSlim(maxParallelTests);

        //sort classes by priority
        IEnumerable<ClassInfo> sortedClasses = testClasses
            .OrderBy(c => PriorityOrder.GetValueOrDefault(c.Priority));

        TestExecuter testExecuter = new TestExecuter();

        //start timer
        Stopwatch stopwatch = Stopwatch.StartNew();

        var allTasks = new List<Task>();

        ConcurrentDictionary<Type, int> finishedTests = new();

        foreach (var testClass in sortedClasses)
        {
            if (testClass.Results == null)
                testClass.Results = new ConcurrentBag<TestResult>();
            else
                testClass.Results.Clear();

            finishedTests[testClass.ClassType] = 0;

            int totalTestsInClass = testClass.Tests.Count;
            int executedInClass = 0;

            //group tests by priority
            var testsByPriority = testClass.Tests
                .GroupBy(t => t.Priority)
                .OrderBy(g => PriorityOrder[g.Key]);

            foreach (var priorityGroup in testsByPriority)
            {
                //execute tests of the class
                foreach (TestInfo testInfo in priorityGroup)
                {
                    executedInClass++;
                    bool isLastTestInClass = (executedInClass == totalTestsInClass);

                    var task = RunOneTest(testExecuter, testInfo, semaphore, testClass);
                    allTasks.Add(task);
                }
            }         
        }

        //wait for all tasks
        await Task.WhenAll(allTasks);

        //stop timer
        stopwatch.Stop();

        Console.WriteLine($"\nParallel: {stopwatch.ElapsedMilliseconds}ms");
    }

    private static async Task RunOneTest(
        TestExecuter testExecuter,
        TestInfo testInfo,
        SemaphoreSlim semaphore,
        ClassInfo testClass
    )
    {
        await semaphore.WaitAsync();

        CancellationToken token = CancellationToken.None;
        CancellationTokenSource? source = null;

        if (testInfo.Timeout.HasValue)
        {
            source = new CancellationTokenSource(TimeSpan.FromMilliseconds(testInfo.Timeout.Value));
            token = source.Token;
        }

        try
        {
            var result = await Task.Run(() =>
                testExecuter.ExecuteTest(testInfo, token));

            testClass.Results.Add(result);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public static void OutputResults(List<ClassInfo> testClasses)
    {
        //sort classes by priority
        IEnumerable<ClassInfo> sortedClasses = testClasses
            .OrderBy(c => PriorityOrder.GetValueOrDefault(c.Priority));

        //output results
        foreach (var testClass in sortedClasses)
        {
            //output test class name and priority
            Console.WriteLine($"Class priority: {testClass.Priority}");
            Console.WriteLine($"Class: {testClass.DisplayName}\n");

            //group tests by priority
            var resultsByPriority = testClass.Results
                .GroupBy(t => t.Priority)
                .OrderBy(g => PriorityOrder[g.Key]);

            foreach (var priorityGroup in resultsByPriority)
            {
                Console.WriteLine($"    Tests priority: {priorityGroup.Key}\n");

                var resultsInGroup = priorityGroup.ToList();
                foreach (var testResult in resultsInGroup)
                {
                    //output test result
                    if (testResult.Status == "PASSED")
                    {
                        Console.WriteLine($"        {testResult.Status}: {testResult.TestName}\n");
                    }
                    else
                    {
                        Console.WriteLine($"        {testResult.Status}: {testResult.TestName}\n        ErrorType: {testResult.ErrorType}\n        ErrorMessage: {testResult.ErrorMessage}\n");
                    }
                        
                }
            }
        }
    }

    public static async Task Main(string[] args)
    {
        //get pass to dll
        string testAssemblyPath = GetPathToDll(args);

        //load tests
        TestsLoader testLoader = new TestsLoader();
        List<ClassInfo> testClasses = testLoader.LoadTests(testAssemblyPath);

        //run tests and output results
        await RunTestsInParallel(testClasses);
        OutputResults(testClasses);

        RunTestsSyncroniosly(testClasses);
        OutputResults(testClasses);
    }
}
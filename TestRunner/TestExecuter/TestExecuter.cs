using System.Reflection;
using TestRunner.Models;
using TestUnit.Attributes;
using TestUnit.Exeptions;

namespace TestRunner.TestExecuter
{
    public class TestExecuter
    {
        private static Dictionary<Type, bool> _classSetupExecuted = new();

        private void ExecuteMethod(object? instance, MethodInfo? method, object[]? parameters, bool isAsync)
        {
            var result = method?.Invoke(instance, parameters);

            if (isAsync && result is Task task)
            {
                task.GetAwaiter().GetResult();
            }
        }

        public TestResult ExecuteTest(TestInfo testInfo, bool needClassCleanup)
        {
            var result = new TestResult
            {
                TestName = testInfo.DisplayName,
                Status = "PASSED"
            };

            object? instance = null;

            try
            {
                if (testInfo.ClassType == null)
                {
                    throw new InvalidOperationException($"ClassType is null for test {testInfo.DisplayName}");
                }
                instance = Activator.CreateInstance(testInfo.ClassType);

                if (testInfo.ClassSetup != null)
                {
                    bool needClassSetup = false;
                    lock (_classSetupExecuted)
                    {
                        if (!_classSetupExecuted.ContainsKey(testInfo.ClassType))
                        {
                            _classSetupExecuted[testInfo.ClassType] = true;
                            needClassSetup = true;
                        }
                    }

                    if (needClassSetup)
                    {
                        ExecuteMethod(instance, testInfo.ClassSetup, null, testInfo.IsAsync);
                    }
                }

                if (testInfo.MethodSetup != null)
                {
                    ExecuteMethod(instance, testInfo.MethodSetup, null, testInfo.IsAsync);
                }

                ExecuteMethod(instance, testInfo.Method, testInfo.Parameters, testInfo.IsAsync);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is AssertionException)
                {
                    result.Status = "FAILED";
                    result.ErrorMessage = ex.InnerException.Message;
                    result.ErrorType = ex.InnerException.GetType().Name;
                }
                else
                {
                    result.Status = "ERROR";
                    result.ErrorMessage = $"Test error: {ex.InnerException?.Message}";
                    result.ErrorType = ex.InnerException?.GetType().Name ?? "Unknown";
                }

            }
            catch (Exception ex)
            {
                result.Status = "ERROR";
                result.ErrorMessage = $"Runner error: {ex.Message}";
                result.ErrorType = ex.GetType().Name;
            }
            finally
            {
                if (instance != null && testInfo.MethodCleanup != null)
                {
                    ExecuteMethod(instance, testInfo.MethodCleanup, null, testInfo.IsAsync);
                }

                if (needClassCleanup && testInfo.ClassCleanup != null)
                {
                    ExecuteMethod(null, testInfo.ClassCleanup, null, testInfo.IsAsync);
                }
            }

            return result;
        }
    }
}

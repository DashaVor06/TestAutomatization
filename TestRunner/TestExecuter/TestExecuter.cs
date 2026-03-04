using System.Reflection;
using TestRunner.Test;
using TestUnit.Attributes;
using TestUnit.Exeptions;

namespace TestRunner.TestExecuter
{
    public class TestExecuter
    {
        private static Dictionary<Type, bool> _classSetupExecuted = new();
        public TestResult ExecuteTest(TestInfo testInfo, bool needClassCleanup)
        {
            var result = new TestResult
            {
                TestName = testInfo.DisplayName,
                Status = "PASSED"
            };

            object instance = null;

            try
            {
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
                        testInfo.ClassSetup.Invoke(instance, null);
                    }
                }

                if (testInfo.MethodSetup != null)
                {
                    testInfo.MethodSetup.Invoke(instance, null);
                }

                testInfo.Method.Invoke(instance, testInfo.Parameters);
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
                    testInfo.MethodCleanup.Invoke(instance, null);
                }

                if (needClassCleanup && testInfo.ClassCleanup != null)
                {
                    testInfo.ClassCleanup.Invoke(null, null);
                }
            }

            return result;
        }
    }
}

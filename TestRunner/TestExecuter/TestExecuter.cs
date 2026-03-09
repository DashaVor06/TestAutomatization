using System.Collections.Concurrent;
using System.Reflection;
using TestRunner.Models;
using TestUnit.Exeptions;

namespace TestRunner.TestExecuter
{
    public class TestExecuter
    {
        private void ExecuteMethod(object? instance, MethodInfo method, object[]? parameters, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool isAsync =
                method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));

            if (isAsync)
            {
                var task = (Task)method.Invoke(instance, parameters)!;
                task.Wait(cancellationToken);
            }
            else
            {
                try
                {
                    Task.Run(() => method.Invoke(instance, parameters), cancellationToken)
                        .Wait(cancellationToken);
                }
                catch (AggregateException ae) when (ae.InnerException is OperationCanceledException)
                {
                    throw ae.InnerException!;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        public TestResult ExecuteTest(TestInfo testInfo, CancellationToken cancellationToken)
        {
            var result = new TestResult
            {
                TestName = testInfo.DisplayName,
                Status = "PASSED",
                Priority = testInfo.Priority
            };

            object? instance = null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                instance = Activator.CreateInstance(testInfo.ClassType);

                if (testInfo.MethodSetup != null)
                {
                    ExecuteMethod(instance, testInfo.MethodSetup, null, cancellationToken);
                }

                // Тест
                ExecuteMethod(instance, testInfo.Method, testInfo.Parameters, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                result.Status = "TIMEOUT";
                result.ErrorMessage = "Test exceeded timeout";
                result.ErrorType = "TimeoutException";
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is OperationCanceledException)
                {
                    result.Status = "TIMEOUT";
                    result.ErrorMessage = "Test exceeded timeout";
                    result.ErrorType = "TimeoutException";
                }
                else if (ex.InnerException is TargetInvocationException targetEx)
                {
                    if (targetEx.InnerException is AssertionException)
                    {
                        result.Status = "FAILED";
                        result.ErrorMessage = targetEx.InnerException.Message;
                        result.ErrorType = targetEx.InnerException.GetType().Name;
                    }
                    else
                    {
                        result.Status = "ERROR";
                        result.ErrorMessage = $"Test error: {targetEx.InnerException?.Message}";
                        result.ErrorType = targetEx.InnerException?.GetType().Name ?? "Unknown";
                    }
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
                // Method cleanup выполняется после каждого теста
                if (instance != null && testInfo.MethodCleanup != null)
                {
                    var cleanupCts = CancellationToken.None;
                    ExecuteMethod(instance, testInfo.MethodCleanup, null, cleanupCts);
                }
            }

            return result;
        }
    }
}
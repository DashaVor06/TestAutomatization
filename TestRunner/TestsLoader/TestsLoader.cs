using System.Reflection;
using TestUnit.Attributes.ClassAttributes;
using TestUnit.Attributes.ClassAttributes.SetupCleanup;
using TestUnit.Attributes.MethodAttributes.Fact;
using TestUnit.Attributes.MethodAttributes.Theory;
using TestUnit.Attributes;
using TestRunner.Models;

namespace TestRunner.TestLoader
{
    public class TestsLoader
    {
        private Dictionary<string, string> GetTraits(MemberInfo member)
        {
            return member.GetCustomAttributes<TestTraitAttribute>()
                        .ToDictionary(attr => attr.Key, attr => attr.Value);
        }

        private string GetDisplayName(Dictionary<string, string> traits, MemberInfo member, string defaultName)
        {
            return traits.TryGetValue("DisplayName", out var displayName) ? displayName : defaultName;
        }

        private string GetPriority(Dictionary<string, string> traits, string defaultValue = "P3")
        {
            return traits.TryGetValue("Priority", out var priority) ? priority : defaultValue;
        }

        private List<TestInfo> SearchFact(
            Type type,
            MethodInfo? classSetup,
            MethodInfo? classCleanup,
            MethodInfo? methodSetup,
            MethodInfo? methodCleanup
        )
        {
            List<TestInfo> tests = new List<TestInfo>();

            foreach (var method in type.GetMethods())
            {
                var factAttribute = method.GetCustomAttribute<FactMethodAttribute>();
                if (factAttribute != null)
                {
                    var methodTraits = GetTraits(method);

                    tests.Add(new TestInfo
                    {
                        ClassType = type,
                        Method = method,
                        DisplayName = GetDisplayName(methodTraits, method, method.Name),
                        Priority = GetPriority(methodTraits),
                        Traits = methodTraits,
                        Parameters = null,
                        ClassSetup = classSetup,
                        ClassCleanup = classCleanup,
                        MethodSetup = methodSetup,
                        MethodCleanup = methodCleanup,
                        IsAsync = (method.ReturnType == typeof(Task) ||
                            (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                    });
                }
            }

            return tests;
        }

        private List<TestInfo> SearchTheory(
            Type type,
            MethodInfo? classSetup,
            MethodInfo? classCleanup,
            MethodInfo? methodSetup,
            MethodInfo? methodCleanup
        )
        {
            List<TestInfo> tests = new List<TestInfo>();

            foreach (var method in type.GetMethods())
            {
                var theoryAttr = method.GetCustomAttribute<TheoryMethodAttribute>();
                if (theoryAttr != null)
                {
                    var inlineDataAttrs = method.GetCustomAttributes<TheoryInlineDataAttribute>().ToList();
                    var methodTraits = GetTraits(method);

                    if (inlineDataAttrs.Any())
                    {
                        foreach (var inlineData in inlineDataAttrs)
                        {
                            tests.Add(new TestInfo
                            {
                                ClassType = type,
                                Method = method,
                                DisplayName = GetDisplayName(methodTraits, method, method.Name),
                                Priority = GetPriority(methodTraits),
                                Traits = methodTraits,
                                Parameters = inlineData.Parameters,
                                ClassSetup = classSetup,
                                ClassCleanup = classCleanup,
                                MethodSetup = methodSetup,
                                MethodCleanup = methodCleanup,
                                IsAsync = (method.ReturnType == typeof(Task) ||
                                    (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                            });
                        }
                    }
                }
            }

            return tests;
        }

        public List<ClassInfo> LoadTests(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            List<ClassInfo> testClasses = new List<ClassInfo>();

            foreach (var type in assembly.GetTypes())
            {
                var testClassAttr = type.GetCustomAttribute<TestClassAttribute>();
                if (testClassAttr != null)
                {
                    var classTraits = GetTraits(type);

                    var classSetup = type.GetMethods()
                        .FirstOrDefault(m => m.GetCustomAttribute<TestClassSetupAttribute>() != null);
                    var classCleanup = type.GetMethods()
                        .FirstOrDefault(m => m.GetCustomAttribute<TestClassCleanupAttribute>() != null);

                    var methodSetup = type.GetMethods()
                       .FirstOrDefault(m => m.GetCustomAttribute<TestSetupAttribute>() != null);
                    var methodCleanup = type.GetMethods()
                        .FirstOrDefault(m => m.GetCustomAttribute<TestCleanupAttribute>() != null);

                    var classTests = new List<TestInfo>();
                    classTests.AddRange(SearchFact(type, classSetup, classCleanup, methodSetup, methodCleanup));
                    classTests.AddRange(SearchTheory(type, classSetup, classCleanup, methodSetup, methodCleanup));

                    var testClassInfo = new ClassInfo
                    {
                        ClassType = type,
                        DisplayName = GetDisplayName(classTraits, type, type.Name),
                        Priority = GetPriority(classTraits),
                        Traits = classTraits,
                        Tests = classTests
                    };

                    testClasses.Add(testClassInfo);
                }
            }

            return testClasses;
        }
    }
}
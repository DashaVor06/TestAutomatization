using System.Reflection;
using TestUnit.Attributes.ClassAttributes;
using TestUnit.Attributes.MethodAttributes.Fact;
using TestUnit.Attributes.MethodAttributes.Theory;
using TestUnit.Attributes;
using TestRunner.Models;
using TestUnit.Attributes.MethodAttributes.SetupCleanup;

namespace TestRunner.TestLoader
{
    public class TestsLoader
    {
        private Dictionary<string, string> GetTraits(MemberInfo member)
        {
            return member.GetCustomAttributes<TestTraitAttribute>()
                        .ToDictionary(attr => attr.Key, attr => attr.Value);
        }

        private string GetDisplayName(Dictionary<string, string> traits, string defaultName)
        {
            return traits.TryGetValue("DisplayName", out var displayName) ? displayName : defaultName;
        }

        private string GetPriority(Dictionary<string, string> traits, string defaultValue = "P3")
        {
            return traits.TryGetValue("Priority", out var priority) ? priority : defaultValue;
        }

        private double? GetTimeout(Dictionary<string, string> traits)
        {
            traits.TryGetValue("Timeout", out var strTimeout);
            if (double.TryParse(strTimeout, out var intTimeout))
            {
                return intTimeout;
            }
            else 
            {
                return null;
            }
        }

        private List<TestInfo> SearchFact(
            Type type,
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
                        DisplayName = GetDisplayName(methodTraits, method.Name),
                        Priority = GetPriority(methodTraits),
                        Timeout = GetTimeout(methodTraits),
                        Traits = methodTraits,
                        Parameters = null,
                        MethodSetup = methodSetup,
                        MethodCleanup = methodCleanup,
                    });
                }
            }

            return tests;
        }

        private List<TestInfo> SearchTheory(
            Type type,
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
                                DisplayName = GetDisplayName(methodTraits, method.Name),
                                Priority = GetPriority(methodTraits),
                                Timeout = GetTimeout(methodTraits),
                                Traits = methodTraits,
                                Parameters = inlineData.Parameters,
                                MethodSetup = methodSetup,
                                MethodCleanup = methodCleanup,
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

                    var methodSetup = type.GetMethods()
                       .FirstOrDefault(m => m.GetCustomAttribute<TestSetupAttribute>() != null);
                    var methodCleanup = type.GetMethods()
                        .FirstOrDefault(m => m.GetCustomAttribute<TestCleanupAttribute>() != null);

                    var classTests = new List<TestInfo>();
                    classTests.AddRange(SearchFact(type, methodSetup, methodCleanup));
                    classTests.AddRange(SearchTheory(type, methodSetup, methodCleanup));

                    var testClassInfo = new ClassInfo
                    {
                        ClassType = type,
                        DisplayName = GetDisplayName(classTraits, type.Name),
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
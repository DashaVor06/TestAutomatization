using System.Reflection;
using TestRunner.Test;
using TestUnit.Attributes.ClassAttributes;
using TestUnit.Attributes.ClassAttributes.SetupCleanup;
using TestUnit.Attributes.MethodAttributes.Fact;
using TestUnit.Attributes.MethodAttributes.Theory;
using TestUnit.Attributes;

namespace TestRunner.TestLoader
{
    public class TestsLoader
    {
        private Dictionary<string, string> GetTraits(MemberInfo member)
        {
            return member.GetCustomAttributes<TestTraitAttribute>()
                        .ToDictionary(attr => attr.Key, attr => attr.Value);
        }

        private Dictionary<string, string> MergeTraits(
            Dictionary<string, string> classTraits,
            Dictionary<string, string> methodTraits)
        {
            var result = new Dictionary<string, string>(classTraits);

            foreach (var kvp in methodTraits)
            {
                result[kvp.Key] = kvp.Value;
            }

            return result;
        }

        private string GetDisplayName(
            Dictionary<string, string> traits,
            MethodInfo method)
        {
            if (traits.TryGetValue("DisplayName", out var displayName))
                return displayName;

            return method.Name;
        }

        private string GetPriority(Dictionary<string, string> traits, string defaultValue = "P3")
        {
            return traits.TryGetValue("Priority", out var priority) ? priority : defaultValue;
        }

        private List<TestInfo> SearchFact(
            Type type,
            MethodInfo classSetup,
            MethodInfo classCleanup,
            MethodInfo methodSetup,
            MethodInfo methodCleanup,
            Dictionary<string, string> classTraits
        )
        {
            List<TestInfo> tests = new List<TestInfo>();

            foreach (var method in type.GetMethods())
            {
                var factAttribute = method.GetCustomAttribute<FactMethodAttribute>();
                var traitAttributes = method.GetCustomAttributes<TestTraitAttribute>();
                var methodTraits = GetTraits(method);

                var allTraits = MergeTraits(classTraits, methodTraits);

                if (factAttribute != null)
                {
                    tests.Add(new TestInfo
                    {
                        ClassType = type,
                        Method = method,
                        DisplayName = GetDisplayName(allTraits, method),
                        Priority = GetPriority(allTraits),
                        Traits = allTraits,
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
            MethodInfo classSetup, 
            MethodInfo classCleanup,
            MethodInfo methodSetup,
            MethodInfo methodCleanup,
            Dictionary<string, string> classTraits
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

                    var allTraits = MergeTraits(classTraits, methodTraits);

                    if (inlineDataAttrs.Any())
                    {
                        foreach (var inlineData in inlineDataAttrs)
                        {
                            tests.Add(new TestInfo
                            {
                                ClassType = type,
                                Method = method,
                                DisplayName = GetDisplayName(allTraits, method),
                                Priority = GetPriority(allTraits),
                                Traits = allTraits,
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

        public List<TestInfo> LoadTests(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            List<TestInfo> tests = new List<TestInfo>();

            foreach (var type in assembly.GetTypes())
            {
                var testClassAttr = type.GetCustomAttribute<TestClassAttribute>();
                if (testClassAttr != null)
                {
                    var classTraits = GetTraits(type);
                    if (classTraits.TryGetValue("DisplayName", out var classDisplayName))
                    {
                        Console.WriteLine($"Класс: {classDisplayName}");
                    }

                    var classSetup = type.GetMethods()
                        .FirstOrDefault(m => m.GetCustomAttribute<TestClassSetupAttribute>() != null);
                    var classCleanup = type.GetMethods()
                        .FirstOrDefault(m => m.GetCustomAttribute<TestClassCleanupAttribute>() != null);

                    var methodSetup = type.GetMethods()
                       .FirstOrDefault(m => m.GetCustomAttribute<TestSetupAttribute>() != null);
                    var methodCleanup = type.GetMethods()
                        .FirstOrDefault(m => m.GetCustomAttribute<TestCleanupAttribute>() != null);

                    tests.AddRange(SearchFact(type, classSetup, classCleanup, methodSetup, methodCleanup, classTraits));
                    tests.AddRange(SearchTheory(type, classSetup, classCleanup, methodSetup, methodCleanup, classTraits));
                }
            }

            return tests;
        }
    }
}

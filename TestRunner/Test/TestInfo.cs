using System.Reflection;

namespace TestRunner.Test
{
    public class TestInfo
    {
        public Type ClassType { get; set; }
        public object[] Parameters { get; set; }
        public bool IsAsync { get; set; }

        public string DisplayName { get; set; } = String.Empty;
        public string Priority { get; set; } = String.Empty;
        public Dictionary<string, string> Traits { get; set; } = new();

        public MethodInfo Method { get; set; }
        public MethodInfo ClassSetup { get; set; }
        public MethodInfo ClassCleanup { get; set; }
        public MethodInfo MethodSetup { get; set; }
        public MethodInfo MethodCleanup { get; set; }
    }
}

using System.Reflection;

namespace TestRunner.Models
{
    public class TestInfo
    {
        public required Type ClassType { get; set; }
        public object[]? Parameters { get; set; }

        public string DisplayName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double? Timeout { get; set; }
        public Dictionary<string, string> Traits { get; set; } = new();

        public required MethodInfo Method { get; set; }
        public MethodInfo? MethodSetup { get; set; }
        public MethodInfo? MethodCleanup { get; set; }
    }
}

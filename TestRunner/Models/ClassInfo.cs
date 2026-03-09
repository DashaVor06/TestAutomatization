using System.Collections.Concurrent;
using TestRunner.Models;

namespace TestRunner.Models
{
    public class ClassInfo
    {
        public required Type ClassType { get; set; }
        public string DisplayName { get; set; } = String.Empty;
        public string Priority { get; set; } = String.Empty;
        public Dictionary<string, string> Traits { get; set; } = new();
        public List<TestInfo> Tests { get; set; } = new();
        public ConcurrentBag<TestResult> Results { get; set; } = new();
    }
}

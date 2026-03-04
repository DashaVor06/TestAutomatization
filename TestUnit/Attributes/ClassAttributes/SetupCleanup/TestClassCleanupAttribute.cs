namespace TestUnit.Attributes.ClassAttributes.SetupCleanup
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestClassCleanupAttribute : Attribute
    {
    }
}

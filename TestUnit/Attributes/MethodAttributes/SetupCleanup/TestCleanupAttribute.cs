namespace TestUnit.Attributes.MethodAttributes.SetupCleanup
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestCleanupAttribute : Attribute
    {
    }
}

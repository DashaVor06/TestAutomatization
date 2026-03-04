namespace TestUnit.Attributes.MethodAttributes.Theory
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TheoryInlineDataAttribute : Attribute
    {
        public object[] Parameters { get; }

        public TheoryInlineDataAttribute(params object[] parameters)
        {
            Parameters = parameters;
        }
    }
}

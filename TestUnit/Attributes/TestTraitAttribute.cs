namespace TestUnit.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class TestTraitAttribute : Attribute
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public TestTraitAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}

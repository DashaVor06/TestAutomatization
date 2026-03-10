namespace TestUnit.Exeptions
{
    public class AssertionException : Exception
    {
        public string? Expected { get; }
        public string? Actual { get; }
        public AssertionException(string message, string? expected, string? actual)
            : base(message)
        {
            Expected = expected;
            Actual = actual;
        }
    }
}

namespace TestUnit.Exeptions
{
    public class AssertionException : Exception
    {
        public string? Expected { get; }
        public string? Actual { get; }
        public string? Expression { get; }

        public AssertionException(string message) : base(message) { }

        public AssertionException(string message, string? expected, string? actual)
            : base(message)
        {
            Expected = expected;
            Actual = actual;
        }

        public AssertionException(string message, string? expression)
            : base(message)
        {
            Expression = expression;
        }
    }
}

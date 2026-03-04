using System;

namespace TestUnit.Exeptions
{
    public class FalseException : AssertionException
    {
        public bool? Actual { get; }

        public FalseException()
            : base("Expected false, but was true")
        {
            Actual = true;
        }

        public FalseException(string message)
            : base(message)
        { }

        public FalseException(bool actual, string expression = null)
            : base(FormatMessage(actual, expression))
        {
            Actual = actual;
        }

        private static string FormatMessage(bool actual, string expression)
        {
            if (!string.IsNullOrEmpty(expression))
                return $"Expected '{expression}' to be false, but was true";

            return "Expected false, but was true";
        }
    }
}
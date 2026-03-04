using System;

namespace TestUnit.Exeptions
{
    public class TrueException : AssertionException
    {
        public bool? Actual { get; }

        public TrueException()
            : base("Expected true, but was false")
        {
            Actual = false;
        }

        public TrueException(string message)
            : base(message)
        { }
    }
}
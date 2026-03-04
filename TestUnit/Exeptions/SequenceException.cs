using System;
using System.Collections;

namespace TestUnit.Exeptions
{
    public class SequenceEqualException : AssertionException
    {
        public SequenceEqualException()
            : base("Sequences are not equal")
        {
        }

        public SequenceEqualException(string message)
            : base(message)
        {
        }
    }
}
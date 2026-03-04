using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit.Exeptions
{
    public class EqualException : AssertionException
    {
        public object? Expected { get; }
        public object? Actual { get; }
        public EqualException(object? actual, object? expected)
            : base($"Expected: {expected}, but was: {actual}")
        {
            Expected = expected;
            Actual = actual;
        }
    }
}

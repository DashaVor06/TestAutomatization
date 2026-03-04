using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit.Exeptions
{
    public class NotEqualException : AssertionException
    {
        public object Expected { get; }
        public object Actual { get; }

        public NotEqualException(object expected, object actual)
            : base($"Expected not: {expected}, but was: {actual}")
        {
            Expected = expected;
            Actual = actual;
        }
    }
}

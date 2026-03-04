using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit.Exeptions
{
    public class NotNullException : AssertionException
    {
        public object Actual { get; }

        public NotNullException(object actual)
            : base($"Expected null, but was: {actual}")
        {
            Actual = actual;
        }
    }
}

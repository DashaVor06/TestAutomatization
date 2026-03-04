using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit.Exeptions
{
    public class AssertionException : Exception
    {
        public AssertionException() : base("Assertion failed") { }
        public AssertionException(string message) : base(message) { }
    }
}

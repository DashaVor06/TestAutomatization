using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit.Exeptions
{
    public class CancelationException : Exception
    {
        public CancelationException(string message) : base(message) { }
    }
}

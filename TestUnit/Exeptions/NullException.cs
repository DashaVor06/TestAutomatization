using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUnit.Exeptions
{
    public class NullException : AssertionException
    {
        public string ParameterName { get; }

        public NullException(string parameterName)
            : base($"Parameter '{parameterName}' is null, but expected not null")
        {
            ParameterName = parameterName;
        }
    }
}

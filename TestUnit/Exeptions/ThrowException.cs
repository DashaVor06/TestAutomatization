using System;

namespace TestUnit.Exeptions
{
    public class ThrowsException : AssertionException
    {
        public Type ExpectedExceptionType { get; }
        public Type? ActualExceptionType { get; }
        public string? ActualExceptionMessage { get; }

        public ThrowsException(Type expectedType)
            : base($"Expected exception {expectedType.Name}, but none was thrown")
        {
            ExpectedExceptionType = expectedType;
            ActualExceptionType = null;
            ActualExceptionMessage = null;
        }

        public ThrowsException(Type expectedType, Exception? actualException)
            : base(FormatMessage(expectedType, actualException))
        {
            ExpectedExceptionType = expectedType;
            ActualExceptionType = actualException?.GetType();
            ActualExceptionMessage = actualException?.Message;
        }

        private static string FormatMessage(Type expectedType, Exception? actualException)
        {
            if (actualException == null)
                return $"Expected exception {expectedType.Name}, but none was thrown";

            return $"Expected exception {expectedType.Name}, but got {actualException.GetType().Name}: {actualException.Message}";
        }
    }
}
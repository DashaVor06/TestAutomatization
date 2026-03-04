using System;

namespace TestUnit.Exeptions
{
    public class TypeException : AssertionException
    {
        public Type ExpectedType { get; }
        public Type? ActualType { get; }
        public bool ExpectedMatch { get; }

        public TypeException(Type expectedType, Type? actualType, bool expectedMatch)
            : base(FormatMessage(expectedType, actualType, expectedMatch))
        {
            ExpectedType = expectedType;
            ActualType = actualType;
            ExpectedMatch = expectedMatch;
        }

        private static string FormatMessage(Type expectedType, Type? actualType, bool expectedMatch)
        {
            string actualTypeName = actualType?.Name ?? "null";

            if (expectedMatch)
            {
                return $"Expected type: {expectedType.Name}, but was: {actualTypeName}";
            }
            else
            {
                return $"Expected not type: {expectedType.Name}, but was: {actualTypeName}";
            }
        }
    }
}
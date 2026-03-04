using TestUnit.Exeptions;

namespace TestUnit.Asserts
{
    public static class TestAssert
    {
        public static void Equal(object? actual, object? expected)
        {
            if (expected == null && actual == null)
                return;

            if (expected == null || actual == null)
                throw new NotEqualException(actual, expected);

            if (!expected.Equals(actual))
                throw new NotEqualException(actual, expected);
        }

        public static void NotEqual(object? actual, object? expected) 
        {
            if (expected == null && actual == null)
                throw new EqualException(actual, expected);

            if (expected == null || actual == null)
                return;

            if (expected.Equals(actual))
                throw new EqualException(actual, expected);
        }

        public static void Null(object? obj)
        {
            if (obj != null) throw new NotNullException(obj);
        }

        public static void NotNull(object? obj)
        {
            if (obj == null) throw new NullException(nameof(obj));
        }

        public static void True(bool condition)
        {
            if (!condition)
                throw new TrueException();
        }

        public static void False(bool condition)
        { 
            if (condition)
                throw new FalseException();
        }
        public static void SequenceEqual<T>(IEnumerable<T>? actual, IEnumerable<T>? expected)
        {
            if (expected == null && actual == null)
                return;

            if (expected == null || actual == null)
                throw new SequenceEqualException("One of the sequences is null");

            if (!expected.SequenceEqual(actual))
                throw new SequenceEqualException("Sequences are not equal");
        }

        public static void IsType<TExpected>(object? obj)
        {
            if (obj == null)
                throw new TypeException(typeof(TExpected), null, true);

            if (!(obj is TExpected))
                throw new TypeException(typeof(TExpected), obj.GetType(), true);
        }

        public static void IsNotType<TExpected>(object? obj)
        {
            if (obj == null)
                return;

            if (obj is TExpected)
                throw new TypeException(typeof(TExpected), obj.GetType(), false);
        }

        public static void Throws<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
                throw new ThrowsException(typeof(TException));
            }
            catch (TException)
            {
            }
            catch (Exception ex)
            {
                throw new ThrowsException(typeof(TException), ex);
            }
        }

    }
}

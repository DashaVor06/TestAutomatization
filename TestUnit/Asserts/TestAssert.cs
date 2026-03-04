using TestUnit.Exeptions;

namespace TestUnit.Asserts
{
    public static class TestAssert
    {
        public static void Equal(object? actual, object? expected, string? message = null)
        {
            if (Equals(expected, actual)) return;

            throw new AssertionException(
                message ?? $"Expected: {FormatValue(expected)}, but was: {FormatValue(actual)}",
                FormatValue(expected),
                FormatValue(actual)
            );
        }

        public static void NotEqual(object? actual, object? expected, string? message = null)
        {
            if (!Equals(expected, actual)) return;

            throw new AssertionException(
                message ?? $"Expected not: {FormatValue(expected)}",
                FormatValue(expected),
                FormatValue(actual)
            );
        }

        public static void Null(object? obj, string? message = null)
        {
            if (obj == null) return;

            throw new AssertionException(
                message ?? $"Expected null, but was: {FormatValue(obj)}",
                "null",
                FormatValue(obj)
            );
        }

        public static void NotNull(object? obj, string? message = null)
        {
            if (obj != null) return;

            throw new AssertionException(
                message ?? "Expected not null, but was null",
                "not null",
                "null"
            );
        }

        public static void True(bool condition, string? message = null)
        {
            if (condition) return;

            throw new AssertionException(
                message ?? "Expected true, but was false",
                "true",
                "false"
            );
        }

        public static void False(bool condition, string? message = null)
        {
            if (!condition) return;

            throw new AssertionException(
                message ?? "Expected false, but was true",
                "false",
                "true"
            );
        }

        public static void SequenceEqual<T>(IEnumerable<T>? actual, IEnumerable<T>? expected, string? message = null)
        {
            if (expected == null && actual == null) return;
            if (expected == null || actual == null)
                throw new AssertionException(
                    message ?? "One of the sequences is null",
                    expected?.ToString() ?? "null",
                    actual?.ToString() ?? "null"
                );

            if (expected.SequenceEqual(actual)) return;

            throw new AssertionException(
                message ?? "Sequences are not equal",
                FormatSequence(expected),
                FormatSequence(actual)
            );
        }

        public static void IsType<TExpected>(object? obj, string? message = null)
        {
            if (obj is TExpected) return;

            throw new AssertionException(
                message ?? $"Expected type: {typeof(TExpected).Name}, but was: {obj?.GetType().Name ?? "null"}",
                typeof(TExpected).Name,
                obj?.GetType().Name ?? "null"
            );
        }

        public static void IsNotType<TExpected>(object? obj, string? message = null)
        {
            if (obj == null)
                return;

            if (obj is TExpected)
            {
                throw new AssertionException(
                    message ?? $"Expected not type: {typeof(TExpected).Name}, but was: {obj.GetType().Name}",
                    $"not {typeof(TExpected).Name}",
                    obj.GetType().Name
                );
            }
        }

        public static void Throws<TException>(Action action, string? message = null) where TException : Exception
        {
            try
            {
                action();
                throw new AssertionException(
                    message ?? $"Expected exception {typeof(TException).Name}, but none was thrown",
                    typeof(TException).Name,
                    "no exception"
                );
            }
            catch (TException)
            {
            }
            catch (Exception ex)
            {
                throw new AssertionException(
                    message ?? $"Expected exception {typeof(TException).Name}, but got {ex.GetType().Name}: {ex.Message}",
                    typeof(TException).Name,
                    ex.GetType().Name
                );
            }
        }

        private static string FormatValue(object? value)
        {
            if (value == null) return "null";
            if (value is string) return $"\"{value}\"";
            return value.ToString() ?? "null";
        }

        private static string FormatSequence<T>(IEnumerable<T> sequence)
        {
            return $"[{string.Join(", ", sequence.Take(5))}{(sequence.Count() > 5 ? "..." : "")}]";
        }
    }
}
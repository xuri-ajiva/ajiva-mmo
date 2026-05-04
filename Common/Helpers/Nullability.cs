using System.Diagnostics;

namespace Common.Helpers;

public static class Nullability
{
    extension<T>(T? value)
    {
        public T NotNull()
        {
            Debug.Assert(value is not null);
            return value!;
        }

        public T ValueOr(T defaultValue)
        {
            return value is not null ? value : defaultValue;
        }
    }
}
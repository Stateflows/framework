using System;

namespace Stateflows.Common.Utilities
{
    public static class ImplicitConverter
    {
        public static bool TryConvert<T>(object value, out T output)
        {
            object tempOutput;
            bool result = TryConvert(typeof(T), value, out tempOutput);
            output = (T)tempOutput;
            return result;
        }

        public static bool TryConvert(Type targetType, object value, out object output)
        {
            output = default;
            if (value.GetType() == targetType)
            {
                output = value;
                return true;
            }

            var converter = targetType.GetMethod("op_Implicit", new[] { value.GetType() });
            if (converter != null)
            {
                output = converter.Invoke(null, new[] { value });
                return true;
            }

            return false;
        }
    }
}

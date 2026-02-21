namespace Genocs.Saga.Errors;

internal static class Check
{
    internal static void Is<TExpected>(Type type, string message = null)
    {
        if (typeof(TExpected).IsAssignableFrom(type))
        {
            return;
        }

        message = message ?? CheckErrorMessages.InvalidArgumentType;
        throw new SagaException(message);
    }

    internal static void IsNull<TData>(TData data, string message = null)
        where TData : class
    {
        if (data is null)
        {
            message = message ?? CheckErrorMessages.ArgumentNull;
            throw new SagaException(message);
        }
    }

    private static class CheckErrorMessages
    {
        public static string InvalidArgumentType = "Invalid argument type";
        public static string ArgumentNull = "Argument null";
    }
}

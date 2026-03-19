namespace Genocs.Messaging;

public class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContextHolder>
        Holder = new();

    public object? CorrelationContext
    {
        get => Holder.Value?.Context;
        set
        {
            var holder = Holder.Value;
            holder?.Context = null;

            if (value != null)
            {
                Holder.Value = new CorrelationContextHolder { Context = value };
            }
        }
    }

    private class CorrelationContextHolder
    {
        public object? Context;
    }
}
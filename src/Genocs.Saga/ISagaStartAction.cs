namespace Genocs.Saga;

public interface ISagaStartAction<in TMessage> : ISagaAction<TMessage>;


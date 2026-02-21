namespace Genocs.Saga.Managers;

internal interface ISagaSeeker
{
    IEnumerable<ISagaAction<TMessage>> Seek<TMessage>();
}

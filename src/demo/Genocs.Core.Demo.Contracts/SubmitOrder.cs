using System;

namespace Genocs.Core.Demo.Contracts
{
    public interface SubmitOrder
    {
        Guid Id { get; }
        public string OrderId { get; }
        public string UserId { get; }
    }
}

namespace Genocs.Core.Builders
{
    using System;

    internal class ServiceId : IServiceId
    {
        public string Id { get; } = $"{Guid.NewGuid():N}";
    }
}
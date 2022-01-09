using Genocs.Core.Demo.Contracts;
using Genocs.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace UTU.Platform.Demo.AzureServiceBus.BusWorker.Handlers
{
    public class DemoSubscription1EventHandler : IEventHandler<DemoEvent>
    {
        private readonly ILogger<DemoSubscription1EventHandler> _logger;

        public DemoSubscription1EventHandler(ILogger<DemoSubscription1EventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleEvent(DemoEvent @event)
        {
            _logger.LogInformation("{0}, {1}", @event.Name, @event.Address);
 
            // Do something with the message here
            return Task.CompletedTask;
        }
    }
}

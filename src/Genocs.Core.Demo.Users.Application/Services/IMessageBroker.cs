using Convey.CQRS.Events;
using System.Threading.Tasks;

namespace Trill.Services.Users.Core.Services;

public interface IMessageBroker
{
    Task PublishAsync(params IEvent[] events);
}
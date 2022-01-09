using Genocs.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace Genocs.Core.Demo.WebApi
{
    public class RequestQueuedEvent : IEvent
    {
        public string RequestId { get; set; }
        public string MemberId { get; set; }
        public string TransactionRefId { get; set; }
        public string PartnerTransactionRefId { get; set; }
        public string Date { get; set; }
        public string AccountId { get; set; }
        public decimal AirMiles { get; set; }
        public string Status { get; set; }
        public JObject ResponseStatus { get; set; }
    }
}

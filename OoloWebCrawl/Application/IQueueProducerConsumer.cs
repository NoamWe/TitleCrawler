using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoloWebCrawl.Application
{
    public interface IQueueProducerConsumer
    {
        Task SendCrawlingRequests(List<string> urls);
    }
}
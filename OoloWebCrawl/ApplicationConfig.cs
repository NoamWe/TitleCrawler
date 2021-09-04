using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoloWebCrawl
{
    public class ApplicationConfig
    {
        public int DefaultMinutesAgo { get; set; }
        public string RabbitMQConnection { get; set; }
        public string RequestQueueName { get; set; }
        public string ResponseQueueName { get; set; }
    }
}

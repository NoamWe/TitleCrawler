using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OoloWebCrawl.Storage;
using OoloWebCrawl.Storage.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OoloWebCrawl.Application
{
    public class QueueProducerConsumer : IQueueProducerConsumer
    {
        private readonly string _requestQueue;
        private readonly string _responseQueue;

        private IConnection _rabbitMqConnection;
        private readonly ITitlesRepository _titles
            ;

        public QueueProducerConsumer(IOptions<ApplicationConfig> config,
            ITitlesRepository titles)
        {
            var connectionEndpoint = config.Value.RabbitMQConnection;
            _requestQueue = config.Value.RequestQueueName;
            _responseQueue = config.Value.ResponseQueueName;

            _titles = titles;
            InitializeSubscription(connectionEndpoint);
        }

        //TODO find a nicer way to get this done in the DI and not in CTOR
        private void InitializeSubscription(string connectionEndpoint)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionEndpoint)
            };
            _rabbitMqConnection = factory.CreateConnection();
            var channel = _rabbitMqConnection.CreateModel();
            channel.QueueDeclare(_responseQueue, true, false, false, null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                ProcessMessage(body);
            };

            channel.BasicConsume(_responseQueue, true, consumer);
        }

        public Task SendCrawlingRequests(List<string> urls)
        {
            var channel = _rabbitMqConnection.CreateModel();
            channel.QueueDeclare(_requestQueue, true, false, false, null);

            foreach (var url in urls)
            {
                var message = new { Url = url };
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                channel.BasicPublish("", _requestQueue, null, body);
            }

            return Task.CompletedTask;
        }

        private void ProcessMessage(byte[] body)
        {
            try
            {
                string jsonString = Encoding.UTF8.GetString(body);

                var message = JsonConvert.DeserializeObject<ResponseDto>(jsonString);

                //TODO automapper
                _titles.Save(new TitleModel { DateCreated = DateTime.Now, Url = message.url, Title = message.title });
            }
            catch (Exception e)
            {
                //TODO real logger
                Console.WriteLine(e);
            }
        }
    }

    internal class ResponseDto
    {
        public string url { get; set; }
        public string title { get; set; }
    }
}
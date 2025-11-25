using Messaging.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace ProcessingService.Messaging
{
    public interface IDocumentsMessageBus
    {
        void ConsumeDocuments(Action<DocumentChunk> handler);
    }

    internal class DocumentsMessageBus : IDocumentsMessageBus
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private AsyncEventingBasicConsumer? _consumer;

        public DocumentsMessageBus(ConnectionFactory connectionFactory,
                                   string exchangeName,
                                   string routingKey)
        {
            this._connectionFactory = connectionFactory;
            this._exchangeName = exchangeName;
            this._routingKey = routingKey;
        }

        public async Task SetupAsync()
        {
            var connection = await this._connectionFactory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: this._exchangeName, type: ExchangeType.Direct);

            var queueDeclareResult = await channel.QueueDeclareAsync();
            string queueName = queueDeclareResult.QueueName;

            await channel.QueueBindAsync(queue: queueName, exchange: this._exchangeName, routingKey: this._routingKey);

            var consumer = new AsyncEventingBasicConsumer(channel);

            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

            this._consumer = consumer;
        }

        public void ConsumeDocuments(Action<DocumentChunk> handler)
        {
            _consumer!.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var documentChunk = JsonSerializer.Deserialize<DocumentChunk>(body);

                handler.Invoke(documentChunk);

                return Task.CompletedTask;
            };
        }
    }
}
using Messaging.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace ProcessingService.Messaging
{
    public interface IDocumentsMessageBus
    {
        void ConsumeDocuments(Func<DocumentChunk, Task> handler);
    }

    internal class DocumentsMessageBus : IDocumentsMessageBus
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _routingKey;
        private AsyncEventingBasicConsumer? _consumer;
        private IChannel? _channel;

        public DocumentsMessageBus(ConnectionFactory connectionFactory,
                                   string exchangeName,
                                   string queueName,
                                   string routingKey)
        {
            this._connectionFactory = connectionFactory;
            this._exchangeName = exchangeName;
            this._queueName = queueName;
            this._routingKey = routingKey;
        }

        public async Task SetupAsync()
        {
            var connection = await this._connectionFactory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            var queueName = this._queueName;


            await channel.ExchangeDeclareAsync(exchange: this._exchangeName, type: ExchangeType.Direct);

            await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false,
                    arguments: null);

            await channel.QueueBindAsync(queue: queueName, exchange: this._exchangeName, routingKey: this._routingKey);

            var consumer = new AsyncEventingBasicConsumer(channel);

            await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);

            this._consumer = consumer;
            this._channel = channel;
        }

        public void ConsumeDocuments(Func<DocumentChunk, Task> handler)
        {
            _consumer!.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var documentChunk =
                        JsonSerializer.Deserialize<DocumentChunk>(ea.Body.Span)
                        ?? throw new InvalidDataException("Invalid document payload");

                    await handler(documentChunk);

                    await _channel!.BasicAckAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failure while processing document chunk: {ex.Message}");
                    Console.Error.WriteLine(ex);
                }
            };
        }
    }
}
using Messaging.Shared;
using RabbitMQ.Client;
using System.Text.Json;

namespace DataCaptureService.Messaging
{
    public interface IDocumentsMessageBus
    {
        Task PublishDocumentAsync(DocumentChunk documentChunk);
    }

    internal class DocumentsMessageBus : IDocumentsMessageBus
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private IChannel? _channel;

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

            this._channel = channel;

            await channel.ExchangeDeclareAsync(exchange: this._exchangeName, type: ExchangeType.Direct);
        }

        public async Task PublishDocumentAsync(DocumentChunk documentChunk)
        {
            var message = JsonSerializer.SerializeToUtf8Bytes(documentChunk);

            await _channel!.BasicPublishAsync(
                exchange: this._exchangeName,
                routingKey: this._routingKey,
                body: message
            );
        }
    }
}
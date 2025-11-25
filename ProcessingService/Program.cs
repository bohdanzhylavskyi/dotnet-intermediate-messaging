using ProcessingService.Messaging;
using ProcessingService.Services;
using RabbitMQ.Client;

namespace ProcessingService
{
    internal class Program
    {
        private const string DocumentsDestinationFolderName = "Documents";
        private const string DocumentsMessageBusExchangeName = "documents-exchange";
        private const string DocumentsMessageBusQueueName = "documents-queue";
        private const string DocumentsMessageBusRoutingKey = "documents";

        static async Task Main(string[] args)
        {
            var documentsMessageBus = ConfigureDocumentsMessageBus();

            await documentsMessageBus.SetupAsync();

            var processingService = new DocumentsProcessingService(
                documentsMessageBus,
                documentsDestinationFolderPath: ResolveDocumentsDestinationFolderPath());

            processingService.Start();

            Console.WriteLine("Processing service is running...");
            Console.ReadLine();
        }

        static private DocumentsMessageBus ConfigureDocumentsMessageBus()
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };

            return new DocumentsMessageBus(
                connectionFactory,
                exchangeName: DocumentsMessageBusExchangeName,
                queueName: DocumentsMessageBusQueueName,
                routingKey: DocumentsMessageBusRoutingKey
            );
        }

        static private string ResolveDocumentsDestinationFolderPath()
        {
            return Path.Combine(AppContext.BaseDirectory, DocumentsDestinationFolderName);
        }
    }
}

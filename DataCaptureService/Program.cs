using DataCaptureService.Messaging;
using DataCaptureService.Services;
using RabbitMQ.Client;

namespace DataCaptureService
{
    internal class Program
    {
        private const string DocumentsSourceFolderName = "Documents";
        private const string DocumentsMessageBusExchangeName = "direct_logs";
        private const string DocumentsMessageBusRoutingKey = "documents";

        static async Task Main(string[] args)
        {
            var documentsSourceFolderPath = ResolveDocumentsSourceFolderPath();

            var documentsSource = new FsDocumentsSource(documentsSourceFolderPath);
            var documentsMessageBus = ConfigureDocumentsMessageBus();

            var documentsTransferService = new DocumentsTransferService(documentsMessageBus);

            var dataCapturingService = new DataCapturingService(
                documentsSource,
                documentsTransferService
            );

            await documentsMessageBus.Setup();
            
            documentsSource.Start();
            dataCapturingService.Start();

            Console.WriteLine("Data Capture service is running...");
            Console.ReadLine();
        }

        static private DocumentsMessageBus ConfigureDocumentsMessageBus()
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };

            return new DocumentsMessageBus(
                connectionFactory,
                exchangeName: DocumentsMessageBusExchangeName,
                routingKey: DocumentsMessageBusRoutingKey
            );
        }

        static private string ResolveDocumentsSourceFolderPath()
        {
            return Path.Combine(AppContext.BaseDirectory, DocumentsSourceFolderName);
        }
    }
}

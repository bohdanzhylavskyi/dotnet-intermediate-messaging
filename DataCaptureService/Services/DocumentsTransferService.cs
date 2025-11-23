using DataCaptureService.Messaging;
using DataCaptureService.Models;
using Messaging.Shared;

namespace DataCaptureService.Services
{
    internal interface IDocumentsTransferService
    {
        Task PublishDocument(Document document); 
    }

    internal class DocumentsTransferService : IDocumentsTransferService
    {
        private readonly DocumentsMessageBus _documentsMessageBus;

        public DocumentsTransferService(DocumentsMessageBus documentsMessageBus)
        {
            this._documentsMessageBus = documentsMessageBus;
        }

        public async Task PublishDocument(Document document)
        {
            var sequenceId = Guid.NewGuid().ToString();
            var chunkSizeInBytes = 256;

            await foreach (var fileChunk in ReadFileAsChunks(document.FullPath, chunkSizeInBytes))
            {
                var chunk = new DocumentChunk()
                {
                    SequenceId = sequenceId,
                    Position = fileChunk.Index + 1,
                    SequenceSize = fileChunk.Total,
                    Body = fileChunk.Data,
                    Filename = document.Filename,
                };

                await this._documentsMessageBus.PublishDocumentAsync(chunk);
            }
        }

        private async IAsyncEnumerable<(byte[] Data, int Index, int Total)> ReadFileAsChunks(string path, int chunkSizeInBytes)
        {
            using FileStream fs = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                chunkSizeInBytes,
                true
            );

            var fileSize = fs.Length;

            int totalChunks = (int)Math.Ceiling(fileSize / (double) chunkSizeInBytes);

            byte[] buffer = new byte[chunkSizeInBytes];
            
            int bytesRead;
            int chunkIndex = 0;

            while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                yield return (Data: buffer[..bytesRead], Index: chunkIndex, Total: totalChunks);

                chunkIndex++;
            }
        }
    }
}

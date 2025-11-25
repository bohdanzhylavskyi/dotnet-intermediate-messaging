using Messaging.Shared;
using ProcessingService.Messaging;
using ProcessingService.Utils;

namespace ProcessingService.Services
{
    internal class DocumentsProcessingService
    {
        private readonly IDocumentsMessageBus _documentsMessageBus;
        private string _documentsDestinationFolderPath;

        public DocumentsProcessingService(
            IDocumentsMessageBus documentsMessageBus,
            string documentsDestinationFolderPath)
        {
            this._documentsMessageBus = documentsMessageBus;
            this._documentsDestinationFolderPath = documentsDestinationFolderPath;
        }

        public void Start()
        {
            _documentsMessageBus.ConsumeDocuments(HandleDocumentChunkReceived);
        }

        private void HandleDocumentChunkReceived(DocumentChunk chunk)
        {
            Console.WriteLine($"Document Chunk Received: fileName: {chunk.Filename}, seqId: {chunk.SequenceId}, position: {chunk.Position}, size: {chunk.SequenceSize}");

            var tempDocumentFilePath = Path.Combine(_documentsDestinationFolderPath, ResolveTempDocumentFilename(chunk.SequenceId));
            var documentFilePath = Path.Combine(_documentsDestinationFolderPath, ResolveDocumentFilename(chunk));

            if (chunk.Position == 1)
            {
                if (chunk.Position == chunk.SequenceSize)
                {
                    FsUtils.CreateFile(
                        filePath: documentFilePath,
                        chunk.Body
                    );

                } else
                {
                    FsUtils.CreateFile(
                        filePath: tempDocumentFilePath,
                        chunk.Body
                    );
                }

                return;
            }

            if (chunk.Position == chunk.SequenceSize)
            {
                FsUtils.AppendToFile(
                    filePath: tempDocumentFilePath,
                    chunk.Body
                );

                FsUtils.MoveFile(tempDocumentFilePath, documentFilePath);

                return;
            }

            FsUtils.AppendToFile(
                filePath: tempDocumentFilePath,
                chunk.Body
            );
        }

        private string ResolveTempDocumentFilename(string documentSequenceId)
        {
            return $"{documentSequenceId}-file.dat";
        }

        private string ResolveDocumentFilename(DocumentChunk chunk) {
            return chunk.Filename;
        }
    }
}

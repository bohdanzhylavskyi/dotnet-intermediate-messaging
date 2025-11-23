using Messaging.Shared;
using ProcessingService.Messaging;

namespace ProcessingService.Services
{
    internal class DocumentsProcessingService
    {
        private readonly IDocumentsMessageBus _documentsMessageBus;
        private string _documentsDestinationFolderPath;

        public DocumentsProcessingService(
            IDocumentsMessageBus documentsTransferMessageBus,
            string documentsDestinationFolderPath)
        {
            this._documentsMessageBus = documentsTransferMessageBus;
            this._documentsDestinationFolderPath = documentsDestinationFolderPath;
        }

        public void Start()
        {
            _documentsMessageBus.Subscribe(HandleDocumentChunkReceived);
        }

        private void HandleDocumentChunkReceived(DocumentChunk chunk)
        {
            Console.WriteLine($"CHUNK RECEIVED: seqId: {chunk.SequenceId}, position: {chunk.Position}, size: {chunk.SequenceSize}");

            var tempDocumentFilePath = Path.Combine(_documentsDestinationFolderPath, ResolveTempDocumentFilename(chunk.SequenceId));
            var documentFilePath = Path.Combine(_documentsDestinationFolderPath, ResolveDocumentFilename(chunk));

            if (chunk.Position == 1)
            {
                if (chunk.Position == chunk.SequenceSize)
                {
                    CreateFile(
                        filePath: documentFilePath,
                        chunk.Body
                    );

                } else
                {
                    CreateFile(
                        filePath: tempDocumentFilePath,
                        chunk.Body
                    );
                }

                return;
            }

            if (chunk.Position == chunk.SequenceSize)
            {
                AppendToFile(
                    filePath: tempDocumentFilePath,
                    chunk.Body
                );

                MoveFile(tempDocumentFilePath, documentFilePath);

                return;
            }

            AppendToFile(
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

        private void CreateFile(string filePath, byte[] content)
        {
            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                var data = content;

                stream.Write(data, 0, data.Length);
            }
        }

        private void AppendToFile(string filePath, byte[] content)
        {
            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                var data = content;

                stream.Write(data, 0, data.Length);
            }
        }

        private void MoveFile(string filePath, string newFilePath)
        {
            File.Move(filePath, newFilePath);
        }
    }
}

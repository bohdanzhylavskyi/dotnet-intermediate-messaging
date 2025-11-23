using DataCaptureService.Models;

namespace DataCaptureService.Services
{
    internal class DataCapturingService
    {
        private readonly IDocumentsSource _documentsSource;
        private readonly IDocumentsTransferService _documentTransferService;

        public DataCapturingService(
            IDocumentsSource documentsSource,
            IDocumentsTransferService documentTransferService)
        {
            this._documentsSource = documentsSource;
            this._documentTransferService = documentTransferService;
        }

        public void Start()
        {
            this._documentsSource.DocumentCreated += HandleNewDocumentCreated;
        }

        private async void HandleNewDocumentCreated(object? sender, DocumentCreatedEventArgs e)
        {
            await this._documentTransferService.PublishDocument(new Document() {
                FullPath = e.FullPath,
                Filename = e.Filename
            });
        }
    }
}

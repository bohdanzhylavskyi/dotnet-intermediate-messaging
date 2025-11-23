namespace DataCaptureService.Services
{
    internal interface IDocumentsSource
    {
        event EventHandler<DocumentCreatedEventArgs>? DocumentCreated;
    }

    public class DocumentCreatedEventArgs
    {
        public required string FullPath;
        public required string Filename;
    }

    internal class FsDocumentsSource : IDocumentsSource, IDisposable
    {
        private readonly string _sourceFolderPath;
        private FileSystemWatcher? _fsWatcher;

        public event EventHandler<DocumentCreatedEventArgs>? DocumentCreated;

        public FsDocumentsSource(string sourceFolderPath)
        {
            this._sourceFolderPath = sourceFolderPath;
        }

        public void Start()
        {
            var watcher = new FileSystemWatcher
            {
                Path = _sourceFolderPath,
                Filter = "*.*",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            watcher.Created += HandleDocumentFileCreated;
            watcher.EnableRaisingEvents = true;

            this._fsWatcher = watcher;
        }

        public void FireDocumentCreated()
        {
            this.DocumentCreated?.Invoke(this, new DocumentCreatedEventArgs() {
                FullPath = "D:\\dot-net-projects\\intermediate-course\\Messaging\\DataCaptureService\\bin\\Debug\\net8.0\\CaptureSource\\test.txt",
                Filename = "test.txt",
            });
        }

        private async void HandleDocumentFileCreated(object sender, FileSystemEventArgs args)
        {
            await WaitForFileAsync(args.FullPath);

            var eventArgs = new DocumentCreatedEventArgs()
            {
                FullPath = args.FullPath,
                Filename = args.Name ?? Guid.NewGuid().ToString(),
            };

            this.DocumentCreated?.Invoke(this, eventArgs);
        }

        private async Task WaitForFileAsync(string path)
        {
            while (true)
            {
                try
                {
                    using (var stream = new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    {
                        return; // file is ready
                    }
                }
                catch (IOException)
                {
                    await Task.Delay(100); // retry
                }
            }
        }

        public void Dispose()
        {
            this._fsWatcher?.Dispose();
        }
    }
}

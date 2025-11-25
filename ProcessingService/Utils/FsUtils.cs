namespace ProcessingService.Utils
{
    internal static class FsUtils
    {
        public static void CreateFile(string filePath, byte[] content)
        {
            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                var data = content;

                stream.Write(data, 0, data.Length);
            }
        }

        public static void AppendToFile(string filePath, byte[] content)
        {
            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                var data = content;

                stream.Write(data, 0, data.Length);
            }
        }

        public static void MoveFile(string filePath, string newFilePath)
        {
            File.Move(filePath, newFilePath);
        }
    }
}

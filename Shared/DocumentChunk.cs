
namespace Messaging.Shared
{
    public class DocumentChunk
    {
        public required string SequenceId { get; set; }
        public required int Position { get; set; }
        public required int SequenceSize { get; set; }
        public required byte[] Body { get; set; }
        public required string Filename { get; set; }
    }
}

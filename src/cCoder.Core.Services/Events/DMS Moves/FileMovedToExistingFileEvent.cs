using cCoder.Core.Services.EventHandlers;

namespace cCoder.Core.Services.Events.DMS_Moves
{
    public class FileMovedToExistingFileEvent : IEvent<FileMovedToExistingFileVO>
    {
        /// <summary>
        /// Moving to Value Objects is not my favourite thing, but it allows to cover a 'domain scenario' where it is by definition a different event if the two properties change
        /// </summary>
        public FileMovedToExistingFileVO Subject { get; set; }
    }
}

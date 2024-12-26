using cCoder.Core.Services.EventHandlers;
using cCoder.Core.Services.Events.DMS_Moves.Value_Objects;

namespace cCoder.Core.Services.Events.DMS_Moves
{
    public class FileMovedToExistingFolderEvent : IEvent<FileMovedToExistingFolderVO>
    {
        public FileMovedToExistingFolderVO Subject { get; set; }
    }
}

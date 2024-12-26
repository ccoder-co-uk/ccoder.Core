using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.EventHandlers;

namespace cCoder.Core.Services.Events.DMS_Moves
{
    public class FileContentDeletedEvent : IEvent<FileContent>
    {
        public FileContent Subject { get; set; }
    }
}

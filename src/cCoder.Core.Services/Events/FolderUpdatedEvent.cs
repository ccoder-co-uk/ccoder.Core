using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.EventHandlers;

namespace cCoder.Core.Services.Events
{
    public class FolderUpdatedEvent : IEvent<Folder>
    {
        public Folder Subject { get; set; }
    }
}
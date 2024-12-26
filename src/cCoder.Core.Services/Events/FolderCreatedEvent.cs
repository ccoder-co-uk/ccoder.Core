using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.EventHandlers;

namespace cCoder.Core.Services.Events
{
    public class FolderCreatedEvent : IEvent<Folder>
    {
        public Folder Subject { get; set; }
    }
}
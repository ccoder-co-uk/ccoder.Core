using cCoder.Core.Services.EventHandlers;

namespace cCoder.Core.Services.Events
{
    public class FileMovedEvent : IEvent<Objects.Entities.DMS.File>
    {
        public Objects.Entities.DMS.File Subject { get; set; }
    }
}
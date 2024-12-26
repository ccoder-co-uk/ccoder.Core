using cCoder.Core.Services.EventHandlers;
using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Services.Events
{
    public class FileDeletedEvent : IEvent<File>
    {
        public File Subject { get; set; }
    }
}
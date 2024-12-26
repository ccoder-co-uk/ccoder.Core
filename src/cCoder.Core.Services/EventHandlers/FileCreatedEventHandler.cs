using cCoder.Core.Services.Events;

namespace cCoder.Core.Services.EventHandlers
{
    public class FileCreatedEventHandler : IEventHandler<FileCreatedEvent, Objects.Entities.DMS.File>
    {
        public Task HandleAsync(FileCreatedEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}

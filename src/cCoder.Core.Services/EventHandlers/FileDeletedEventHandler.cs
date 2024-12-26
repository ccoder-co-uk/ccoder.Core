using cCoder.Core.Objects;
using cCoder.Core.Services.Events;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.EventHandlers
{
    public class FileDeletedEventHandler(ICoreDataContext context,
        ILogger<FileDeletedEventHandler> logger) : IEventHandler<FileDeletedEvent, Objects.Entities.DMS.File>
    {
        public async Task HandleAsync(FileDeletedEvent @event)
        {
            logger.LogDebug($"Deleting file with id {@event.Subject.Id}");

            context.DeleteFile(@event.Subject.Id);
        }
    }
}

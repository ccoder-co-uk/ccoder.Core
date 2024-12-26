using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.Events;
using cCoder.Core.Services.Events.DMS_Moves;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.EventHandlers
{
    public class FileContentDeletedEventHandler(ICoreDataContext db,
        ILogger<FileContentDeletedEventHandler> logger,
        IEventService eventService) : IEventHandler<FileContentDeletedEvent, FileContent>
    {
        public async Task HandleAsync(FileContentDeletedEvent @event)
        {
            logger.LogDebug($"Deleting file content with id {@event.Subject.Id}");

            //TODO: So we have a DeleteFile method but not a DeleteFileContent method?
            await db.DeleteAsync(@event.Subject);

            //If the version is less than or equal to 1 then it's logically the last one
            if (@event.Subject.Version <= 1)
            {
                await eventService.RaiseEventAsync<FileDeletedEvent, Objects.Entities.DMS.File>(new FileDeletedEvent
                {
                    Subject = @event.Subject.File
                });
            }
        }
    }
}

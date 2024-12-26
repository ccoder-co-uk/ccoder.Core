using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.Events;
using cCoder.Core.Services.Events.DMS_Moves;
using cCoder.Core.Services.Events.DMS_Moves.Value_Objects;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.EventHandlers.DMS_Moves
{
    public class FileMovedToExistingFolderEventHandler(IEventService eventService,
        ILogger<FileMovedToExistingFolderEventHandler> logger) : IEventHandler<FileMovedToExistingFolderEvent, FileMovedToExistingFolderVO>
    {
        public async Task HandleAsync(FileMovedToExistingFolderEvent @event)
        {
            logger.LogDebug($"Moving file with id {@event.Subject.File.Id} to folder {@event.Subject.DestinationFolder.Id} and changing name to {@event.Subject.DesiredPath.Name} from {@event.Subject.File.Name}");

            //DMS/Test.txt?moveTo=Content/Test2.txt
            @event.Subject.File.FolderId = @event.Subject.DestinationFolder.Id;

            @event.Subject.File.Name = @event.Subject.DesiredPath.Name;

            @event.Subject.File.RecomputePath();

            await eventService.RaiseEventAsync<FileUpdatedEvent, Objects.Entities.DMS.File>
            (
                new FileUpdatedEvent 
                {
                    Subject = @event.Subject.File,
                }
            );
        }
    }
}

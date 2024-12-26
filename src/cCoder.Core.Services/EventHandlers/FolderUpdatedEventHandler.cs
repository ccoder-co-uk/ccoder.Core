using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.Events;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.EventHandlers
{
    public class FolderUpdatedEventHandler(ICoreDataContext db,
        IEventService eventService,
        ILogger<FolderUpdatedEventHandler> logger)
        : IEventHandler<FolderUpdatedEvent, Folder>
    {
        public async Task HandleAsync(FolderUpdatedEvent @event)
        {
            logger.LogDebug($"Updating folder with id {@event.Subject.Id}");

            var parent = db
                .GetAll<Folder>()
                .FirstOrDefault(f => f.Id == @event.Subject.ParentId);

            @event.Subject.Parent = parent;

            @event.Subject.RecomputePaths();

            var subFolders = db
                .GetAll<Folder>()
                .Where(f => f.ParentId == @event.Subject.Id)
                .ToList();

            foreach(var subFolder in subFolders)
            {
                await eventService.RaiseEventAsync<FolderUpdatedEvent, Folder>(
                    new FolderUpdatedEvent
                    {
                        Subject = new Folder 
                        {
                            Id = subFolder.Id,
                            AppId = subFolder.AppId,
                            Name = subFolder.Name,
                            ParentId = subFolder.ParentId,
                            Path = subFolder.Path
                        }
                    }
                );
            }

            var subFiles = db
                .GetAll<Objects.Entities.DMS.File>()
                .Where(f => f.FolderId == @event.Subject.Id)
                .ToList();

            foreach(var file in subFiles)
            {
                await eventService.RaiseEventAsync<FileUpdatedEvent, Objects.Entities.DMS.File>
                (
                    new FileUpdatedEvent
                    {
                        Subject = file
                    }
                );
            }
        }
    }
}

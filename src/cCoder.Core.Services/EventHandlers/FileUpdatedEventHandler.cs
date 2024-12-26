using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.EventHandlers
{
    public class FileUpdatedEventHandler(ICoreDataContext db, 
        ILogger<FileUpdatedEventHandler> logger
    ) : IEventHandler<FileUpdatedEvent, Objects.Entities.DMS.File>
    {
        public async Task HandleAsync(FileUpdatedEvent @event)
        {
            logger.LogDebug($"Updating file with id {@event.Subject.Id}");

            var folder = db.GetAll<Folder>()
                .IgnoreQueryFilters()
                .FirstOrDefault(f => f.Id == @event.Subject.FolderId);

            @event.Subject.Folder = folder;

            @event.Subject.RecomputePath();

            await db.UpdateAsync(
                new Objects.Entities.DMS.File 
                {
                    Id = @event.Subject.Id,
                    Name = @event.Subject.Name,
                    CreatedBy = @event.Subject.CreatedBy,
                    CreatedOn = @event.Subject.CreatedOn,
                    Description = @event.Subject.Description,
                    FolderId = @event.Subject.FolderId,
                    MimeType = @event.Subject.MimeType,
                    Path = @event.Subject.Path,
                    Size = @event.Subject.Size
                }
            );
        }
    }
}


using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.Events;
using cCoder.Core.Services.Events.DMS_Moves;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Services.EventHandlers.DMS_Moves
{
    public class FileMovedToExistingFileEventHandler(ICoreDataContext db, 
        IEventService eventService,
        ILogger<FileMovedToExistingFileEventHandler> logger) 
        : IEventHandler<FileMovedToExistingFileEvent, FileMovedToExistingFileVO>
    {
        public async Task HandleAsync(FileMovedToExistingFileEvent @event)
        {
            //DMS/Test.txt?moveTo=Test2.txt and Test2 already exists
            logger.LogDebug($"Pulling existing file contents for file {@event.Subject.ExistingFile.Id}");

            var existingVersions = db.GetAll<FileContent>(false)
                .IgnoreQueryFilters()
                .Where(fc => fc.FileId == @event.Subject.ExistingFile.Id)
                .OrderBy(fc => fc.Version)
                .ToList();

            //TODO: If this is slow, potential refactor is to move versioning information out of file content and have the blob sit
            //in isolated table as application has to download all file contents from database server to move them... inefficient

            logger.LogDebug($"Pulling moved file contents for file {@event.Subject.MovedFile.Id}");

            var movedVersions = db.GetAll<FileContent>(false)
                .IgnoreQueryFilters()
                .Where(fc => fc.FileId == @event.Subject.MovedFile.Id)
                .OrderBy(fc => fc.Version)
                .ToList();

            List<FileContent> contentsToAdd = movedVersions.Select(mv => new FileContent
                {
                    Id = Guid.Empty,
                    CreatedBy = mv.CreatedBy,
                    CreatedOn = mv.CreatedOn,
                    Description = mv.Description,
                    FileId = @event.Subject.ExistingFile.Id,
                    RawData = mv.RawData,
                    Size = mv.Size,
                    Version = 0
                }).ToList();

            //Order by the created on to effectively recompute the version information, latest created on = newest desired version

            var mergedSets = contentsToAdd
                .Union(existingVersions)
                .OrderBy(a => a.CreatedOn)
                .ToList();

            for (int i = 0; i < mergedSets.Count; i++)
            {
                mergedSets[i].Version = i + 1;
            }

            foreach(var entry in mergedSets)
            {
                if (entry.Id == Guid.Empty)
                {
                    var addedVersion = await db.AddAsync(entry);
                    logger.LogDebug($"Added file version {addedVersion.Version} with id {addedVersion.Id} for existing file {entry.FileId}");
                }
                else
                {
                    var updatedVersion = await db.UpdateAsync(entry);
                    logger.LogDebug($"Updated file version {updatedVersion.Version} with id {updatedVersion.Id} for existing file {entry.FileId}");
                }
            }

            logger.LogDebug($"Raising delete event for file {@event.Subject.MovedFile.Id}");

            await eventService.RaiseEventAsync<FileDeletedEvent, File>(new FileDeletedEvent { Subject = @event.Subject.MovedFile });
        }
    }
}

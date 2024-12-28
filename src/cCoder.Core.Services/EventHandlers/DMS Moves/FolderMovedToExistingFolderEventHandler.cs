using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.Events;
using cCoder.Core.Services.Events.DMS_Moves;
using cCoder.Core.Services.Events.DMS_Moves.Value_Objects;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.EventHandlers.DMS_Moves
{
    public class FolderMovedToExistingFolderEventHandler(IEventService eventService, 
        ICoreDataContext db,
        ILogger<FolderMovedToExistingFolderEventHandler> logger) : IEventHandler<FolderMovedToExistingFolderEvent, FolderMovedToExistingFolderVO>
    {
        public async Task HandleAsync(FolderMovedToExistingFolderEvent @event)
        {
            await HandleMovingFolders(eventService, db, logger, @event);
            await HandleMovingFiles(eventService, db, logger, @event);

            await eventService.RaiseEventAsync<FolderDeletedEvent, Folder>(new FolderDeletedEvent
            {
                Subject = @event.Subject.SourceFolder
            });
        }

        private static async Task HandleMovingFolders(IEventService eventService, ICoreDataContext db, ILogger<FolderMovedToExistingFolderEventHandler> logger, FolderMovedToExistingFolderEvent @event)
        {
            var sourceFolders = db
                .GetAll<Folder>()
                .Where(f => f.ParentId == @event.Subject.SourceFolder.Id)
                .OrderBy(f => f.Name)
                .ToList();

            logger.LogDebug($"Source sub folders in {@event.Subject.SourceFolder.Id}: {string.Join(",", sourceFolders.Select(sf => $"{sf.Name} ({sf.Id})"))}");

            var destinationFolders = db
                .GetAll<Folder>()
                .Where(f => f.ParentId == @event.Subject.SourceFolder.Id)
                .OrderBy(f => f.Name)
                .ToList();

            logger.LogDebug($"Destination sub folders in {@event.Subject.DestinationFolder.Id}: {string.Join(",", destinationFolders.Select(sf => $"{sf.Name} ({sf.Id})"))}");

            var newFolders = sourceFolders
                .Where(sf => !destinationFolders.Any(df => df.Name.ToLower() == sf.Name.ToLower()))
                .ToList();

            logger.LogDebug($"New sub folders between {@event.Subject.SourceFolder.Id} -> {@event.Subject.DestinationFolder.Id}: {string.Join(",", newFolders.Select(sf => $"{sf.Name} ({sf.Id})"))}");

            foreach (var entry in newFolders)
            {
                logger.LogDebug($"Raising file moved to existing folder event for source path {entry.Path}");

                await eventService.RaiseEventAsync<FolderMovedToNewFolderEvent, FolderMovedToNewFolderVO>(new FolderMovedToNewFolderEvent
                {
                    Subject = new FolderMovedToNewFolderVO
                    {
                        DesiredPath = new Objects.Path(@event.Subject.DestinationFolder.Path.ToLower() + "/" + entry.Name),
                        Folder = entry
                    }
                });
            }

            var existingFolders = destinationFolders
                .Where(sf => sourceFolders.Any(df => df.Name.ToLower() == sf.Name.ToLower()))
                .ToList();

            logger.LogDebug($"Existing sub folders between {@event.Subject.SourceFolder.Id} -> {@event.Subject.DestinationFolder.Id}: {string.Join(",", existingFolders.Select(sf => $"{sf.Name} ({sf.Id})"))}");

            foreach (var existingFolder in existingFolders)
            {
                logger.LogDebug($"Raising folder moved to existing folder event for folder {existingFolder.Id}");

                var movedFolder = sourceFolders.First(df => df.Name.ToLower() == existingFolder.Name.ToLower());

                await eventService.RaiseEventAsync<FolderMovedToExistingFolderEvent, FolderMovedToExistingFolderVO>(new FolderMovedToExistingFolderEvent
                {
                    Subject = new FolderMovedToExistingFolderVO
                    {
                        DestinationFolder = existingFolder,
                        SourceFolder = movedFolder
                    }
                });
            }
        }

        private static async Task HandleMovingFiles(IEventService eventService, ICoreDataContext db, ILogger<FolderMovedToExistingFolderEventHandler> logger, FolderMovedToExistingFolderEvent @event)
        {
            var sourceFiles = db
                .GetAll<Objects.Entities.DMS.File>(false)
                .Where(f => f.FolderId == @event.Subject.SourceFolder.Id)
                .OrderBy(f => f.Name)
                .ToList();

            logger.LogDebug($"Source files in {@event.Subject.SourceFolder.Id}: {string.Join(",", sourceFiles.Select(sf => $"{sf.Name} ({sf.Id})"))}");

            var destinationFiles = db
                .GetAll<Objects.Entities.DMS.File>(false)
                .Where(f => f.FolderId == @event.Subject.DestinationFolder.Id)
                .OrderBy(f => f.Name)
                .ToList();

            logger.LogDebug($"Destination files in {@event.Subject.DestinationFolder.Id}: {string.Join(",", destinationFiles.Select(sf => $"{sf.Name} ({sf.Id})"))}");

            var newFiles = sourceFiles
                .Where(sf => !destinationFiles.Any(df => df.Name.ToLower() == sf.Name.ToLower()))
                .ToList();

            logger.LogDebug($"New files between {@event.Subject.SourceFolder.Id} -> {@event.Subject.DestinationFolder.Id}: {string.Join(",", newFiles.Select(sf => $"{sf.Name} ({sf.Id})"))}");

            foreach (var entry in newFiles)
            {
                logger.LogDebug($"Raising file moved to existing folder event for source path {entry.Path}");

                await eventService.RaiseEventAsync<FileMovedToExistingFolderEvent, FileMovedToExistingFolderVO>(new FileMovedToExistingFolderEvent
                {
                    Subject = new FileMovedToExistingFolderVO
                    {
                        DestinationFolder = @event.Subject.DestinationFolder,
                        DesiredPath = new Objects.Path(@event.Subject.DestinationFolder.Path.ToLower() + "/" + entry.Name),
                        File = entry
                    }
                });
            }

            var existingFiles = destinationFiles
                .Where(sf => sourceFiles.Any(df => df.Name.ToLower() == sf.Name.ToLower()))
                .ToList();

            logger.LogDebug($"Existing files between {@event.Subject.SourceFolder.Id} -> {@event.Subject.DestinationFolder.Id}: {string.Join(",", existingFiles.Select(sf => $"{sf.Name} ({sf.Id})"))}");

            foreach (var existingFile in existingFiles)
            {
                logger.LogDebug($"Raising file moved to existing file event for file {existingFile.Id}");

                var movedFile = sourceFiles.First(df => df.Name.ToLower() == existingFile.Name.ToLower());

                await eventService.RaiseEventAsync<FileMovedToExistingFileEvent, FileMovedToExistingFileVO>(new FileMovedToExistingFileEvent
                {
                    Subject = new FileMovedToExistingFileVO
                    {
                        ExistingFile = existingFile,
                        MovedFile = movedFile
                    }
                });
            }
        }
    }
}


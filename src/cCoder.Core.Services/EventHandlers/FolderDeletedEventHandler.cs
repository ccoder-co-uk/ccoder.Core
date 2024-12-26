using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.EventHandlers
{
    public class FolderDeletedEventHandler(IEventService eventService, ILogger<FolderDeletedEventHandler> logger, ICoreDataContext context) : IEventHandler<FolderDeletedEvent, Folder>
    {
        public async Task HandleAsync(FolderDeletedEvent @event)
        {
            var subFiles = context
                .GetAll<Objects.Entities.DMS.File>()
                .IgnoreQueryFilters()
                .Where(f => f.FolderId == @event.Subject.Id)
                .ToList();

            foreach (var file in subFiles)
            {
                logger.LogDebug($"Deleting file at path {file.Path} with Id {file.Id}");
                await eventService.RaiseEventAsync<FileDeletedEvent, Objects.Entities.DMS.File>(new FileDeletedEvent { Subject = file });
            }

            var subFolders = context
                .GetAll<Folder>()
                .IgnoreQueryFilters()
                .Where(f => f.ParentId == @event.Subject.Id)
                .ToList();

            foreach(var folder in subFolders)
            {
                logger.LogDebug($"Deleting folder at path {folder.Path} with Id {folder.Id}");

                await eventService.RaiseEventAsync<FolderDeletedEvent, Folder>(new FolderDeletedEvent { Subject = folder });
            }

            var folderRoles = context
                .GetAll<FolderRole>()
                .IgnoreQueryFilters()
                .Where(fr => fr.FolderId == @event.Subject.Id)
                .ToList();

            foreach (var role in folderRoles)
            {
                logger.LogDebug($"Deleting folder role with role id {role.RoleId} for folder {role.FolderId}");

                await context.DeleteAsync(role);
            }

            logger.LogDebug($"Deleting folder {@event.Subject.Id}");

            await context.DeleteAsync(@event.Subject);
        }
    }
}

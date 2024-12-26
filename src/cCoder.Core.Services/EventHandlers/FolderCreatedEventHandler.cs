using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services.Events;
using Microsoft.Extensions.Logging;

namespace cCoder.Core.Services.EventHandlers
{
    public class FolderCreatedEventHandler(ICoreDataContext db, 
        ILogger<FolderCreatedEventHandler> logger) : IEventHandler<FolderCreatedEvent, Folder>
    {
        public async Task HandleAsync(FolderCreatedEvent @event)
        {
            if (@event.Subject.ParentId == null)
            {
                logger.LogDebug($"Creating root level folder {@event.Subject.Name}");

                await HandleRootFolderCreation(@event.Subject);
            }
            else
            {
                logger.LogDebug($"Creating child folder {@event.Subject.Name} for parent {@event.Subject.ParentId}");

                await HandleChildFolderCreation(@event.Subject);
            }
        }

        async Task HandleRootFolderCreation(Folder folder)
        {
            var folderToAdd = new Folder
            {
                AppId = folder.AppId,
                Name = folder.Name,
            };

            folderToAdd.RecomputePaths();

            var newFolder = await db.AddAsync(folderToAdd);

            var appRoles = db.GetAll<Role>()
                .Where(r => r.AppId == folder.AppId)
                .ToList();

            foreach(var role in appRoles)
                await db.AddAsync(new FolderRole { FolderId = newFolder.Id, RoleId = role.Id });
        }

        async Task HandleChildFolderCreation(Folder folder)
        {
            var parent = db.GetAll<Folder>()
                .FirstOrDefault(f => f.Id == folder.ParentId);

            var folderToAdd = new Folder
            {
                AppId = folder.AppId,
                Name = folder.Name,
                ParentId = folder.ParentId,
                Parent = parent
            };

            folderToAdd.RecomputePaths();

            var newFolder = await db.AddAsync(folderToAdd);

            var parentRoles = db.GetAll<FolderRole>()
                .Where(r => r.FolderId == folder.ParentId)
                .ToList();

            foreach (var role in parentRoles)
                await db.AddAsync(new FolderRole { FolderId = newFolder.Id, RoleId = role.RoleId });
        }
    }
}

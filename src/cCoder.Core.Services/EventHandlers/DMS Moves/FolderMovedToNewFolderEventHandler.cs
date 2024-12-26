using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Services.Events;
using cCoder.Core.Services.Events.DMS_Moves;
using cCoder.Core.Services.Events.DMS_Moves.Value_Objects;

namespace cCoder.Core.Services.EventHandlers.DMS_Moves
{
    public class FolderMovedToNewFolderEventHandler(IEventService eventService, ICoreDataContext db) : IEventHandler<FolderMovedToNewFolderEvent, FolderMovedToNewFolderVO>
    {
        public async Task HandleAsync(FolderMovedToNewFolderEvent @event)
        {
            Folder previousFolder = await BuildPath(eventService, db, @event);

            @event.Subject.Folder.Name = @event.Subject.DesiredPath.Name;
            @event.Subject.Folder.ParentId = previousFolder.Id;

            await eventService.RaiseEventAsync<FolderUpdatedEvent, Folder>(new FolderUpdatedEvent
            {
                Subject = @event.Subject.Folder
            });
        }

        private static async Task<Folder> BuildPath(IEventService eventService, ICoreDataContext db, FolderMovedToNewFolderEvent @event)
        {
            string currentPath = string.Empty;
            Folder previousFolder = null;

            for (int i = 0; i < @event.Subject.DesiredPath.Segments.Count() - 1; i++)
            {
                currentPath += "/" + @event.Subject.DesiredPath.Segments[i];

                if (i == 0)
                    currentPath = currentPath.Substring(1);

                var folder = db.GetAll<Folder>()
                    .Where(f => f.AppId == @event.Subject.Folder.AppId && f.Path.ToLower() == currentPath)
                    .FirstOrDefault();

                if (folder == null)
                {
                    await eventService.RaiseEventAsync<FolderCreatedEvent, Folder>(new FolderCreatedEvent
                    {
                        Subject = new Folder
                        {
                            Name = currentPath,
                            AppId = @event.Subject.Folder.AppId,
                            ParentId = previousFolder.Id
                        }
                    });

                    folder = db.GetAll<Folder>()
                        .Where(f => f.AppId == @event.Subject.Folder.AppId && f.Path.ToLower() == currentPath)
                        .FirstOrDefault();
                }

                previousFolder = folder;
            }

            return previousFolder;
        }
    }
}

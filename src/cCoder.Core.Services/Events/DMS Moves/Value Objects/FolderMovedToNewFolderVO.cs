using cCoder.Core.Objects.Entities.DMS;

namespace cCoder.Core.Services.Events.DMS_Moves.Value_Objects
{
    public class FolderMovedToNewFolderVO
    {
        public Folder Folder { get; set; }
        public Objects.Path DesiredPath { get; set; }
    }
}

using cCoder.Core.Objects.Entities.DMS;

namespace cCoder.Core.Services.Events.DMS_Moves.Value_Objects
{
    public class FolderMovedToExistingFolderVO
    {
        public Folder SourceFolder { get; set; }
        public Folder DestinationFolder { get; set; }
    }
}


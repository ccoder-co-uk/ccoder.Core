namespace cCoder.Core.Services.Events.DMS_Moves.Value_Objects
{
    public class FileMovedToExistingFolderVO
    {
        public Objects.Entities.DMS.File File { get; set; }
        public Objects.Path DesiredPath { get; set; }
        public Objects.Entities.DMS.Folder DestinationFolder { get; set; }
    }
}

using File = cCoder.Core.Objects.Entities.DMS.File;

namespace cCoder.Core.Services.Events.DMS_Moves
{
    public class FileMovedToExistingFileVO
    {
        public File MovedFile { get; set; }
        public File ExistingFile { get; set; }
    }
}

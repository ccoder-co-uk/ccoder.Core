using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using System;
using System.Threading.Tasks;

namespace cCoder.Core.Services.DMS
{
    public class FileContentService : CoreService<FileContent>, ICoreService<FileContent>
    {
        public FileContentService(ICoreDataContext db) : base(db) { }

        public override Task<FileContent> AddAsync(FileContent entity) => throw new InvalidOperationException("To create a file content, please post to /API/DMS/{path}");
        public override Task<FileContent> UpdateAsync(FileContent entity) => throw new InvalidOperationException("File content's can't be updated");
        public override Task DeleteAsync(object id) => throw new InvalidOperationException("To delete a file content, please DELETE to /API/DMS/{path}?version=VERSION_NUMBER");
    }
}

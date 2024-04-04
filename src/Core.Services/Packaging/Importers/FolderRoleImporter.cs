using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.DMS;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using cCoder.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cCoder.Core.Packaging.Importers
{
    public class FolderRoleImporter : CoreImporter<FolderRole>
    {
        protected ICoreDataContext Db { get; }

        public FolderRoleImporter(ICoreService<FolderRole> service, ICoreDataContext db) : base(service, "Core/FolderRole") { Db = db; Order = 2; }

        public class FolderRoleInfo
        {
            public string Path { get; set; }
            public string Name { get; set; }
        }

        public override async Task Import(int appId, PackageItem item)
        {
            FolderRoleInfo[] folderRoleInfos = item.Data != null && item.Data.StartsWith("{")
                ? new[] { item.Unpack<FolderRoleInfo>() }
                : item.Unpack<FolderRoleInfo[]>();

            Role[] roles = Db.GetAll<Role>(false)
                .Where(r => r.AppId == appId)
                .ToArray();

            Folder[] folders = Db.GetAll<Folder>(false)
                .Where(r => r.AppId == appId)
                .ToArray();

            List<FolderRole> folderRolesToAdd = new List<FolderRole>();

            folderRoleInfos.ForEach(folderRoleInfo =>
            {
                Folder folder = folders.FirstOrDefault(f => f.Path == folderRoleInfo.Path);
                Role role = roles.FirstOrDefault(r => r.Name == folderRoleInfo.Name);
                if (folder != null && role != null)
                    folderRolesToAdd.Add(new FolderRole { RoleId = role.Id, FolderId = folder.Id });
            });

            await Service.AddAllAsync(folderRolesToAdd);
        }
    }
}


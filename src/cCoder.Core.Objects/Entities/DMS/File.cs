using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace cCoder.Core.Objects.Entities.DMS;

[Table("Files", Schema = "DMS")]
[Parent("Folder")]
public class File
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey("Folder")]
    public Guid FolderId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    [Required]
    public string Path { get; set; }

    [Required]
    public string MimeType { get; set; }

    public string CreatedBy { get; set; }

    [StringLength(10)]
    public string Size { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public Folder Folder { get; set; }

    public ICollection<FileContent> Contents { get; set; } = new List<FileContent>();

    [DontPrivilege]
    public Stream GetContent(int version = 0) => version > 0
            ? new MemoryStream(Contents.FirstOrDefault(c => c.Version == version)?.RawData)
            : (Stream)new MemoryStream(Contents.OrderBy(c => c.Version).Last().RawData);

    [DontPrivilege]
    public void RecomputePath() => Path = FolderId != Guid.Empty
            ? $"{Folder.Path}/{Name}".ToLower()
            : Name.ToLower();

    public void UpdateContents(User user, ICollection<FileContent> newContents, ICoreDataContext core)
    {
        if (user.IsAdminOfApp(Folder.AppId) || UserCan(user, "file_update"))
        {
            // apply updates
            Contents.ForEach(c => c.Version = newContents?.FirstOrDefault(nc => c.Id == nc.Id)?.Version ?? c.Version);
        }

        Contents.ForEach(c => c.RawData = newContents?.FirstOrDefault(nc => c.Id == nc.Id)?.RawData ?? c.RawData);
        Contents.ForEach(c => c.Description = newContents?.FirstOrDefault(nc => c.Id == nc.Id)?.Description ?? c.Description);

        // add the new stuff
        IEnumerable<FileContent> addedContents = newContents?.Where(nc => !Contents.Any(c => c.Id == nc.Id)) ?? Array.Empty<FileContent>();

        addedContents.ForEach(c =>
        {
            c.FileId = Id;
            c.Id = Guid.NewGuid();
            c.CreatedOn = DateTimeOffset.UtcNow;
            c.CreatedBy = user.Id;
            Contents.Add(c);
        });

        // remove the old stuff
        IEnumerable<FileContent> removedContents = Contents.Where(c => !(newContents?.Any(nc => nc.Id == c.Id) ?? false));
        removedContents.ForEach(c => Contents.Remove(c));
        _ = core.DeleteAllAsync(removedContents);
    }

    [DontPrivilege]
    public XElement ToWebDavResponse(string urlBase, XNamespace ns, IEnumerable<string> requestedProperties)
    {
        XElement propStat = BuildPropStatResponse(ns, requestedProperties);

        XElement response = new(ns + "response", new XElement(ns + "href", $"{urlBase}Core/App({Folder.AppId})/DAV/{Path}"), propStat);
        List<string> unsupportedProps = new() { "executable", "checked-in", "checked-out" };

        requestedProperties.Where(unsupportedProps.Contains).ForEach((entry) =>
        {
            response.Add(new XElement(ns + "propStat",
                new XElement(ns + "prop", new XElement(ns + entry)),
                new XElement(ns + "status", "HTTP/1.1 404 Not Found"),
                new XElement(ns + "responsedescription", $"Property {{DAV:}}{entry} is not supported.")));
        });

        return response;
    }

    private XElement BuildPropStatResponse(XNamespace ns, IEnumerable<string> requestedProperties)
        => new(ns + "propstat",
            new XElement(ns + "prop",
                (!requestedProperties.Any() || requestedProperties.Contains("creationdate")) ? new XElement(ns + "creationdate", CreatedOn.ToString("s") + "Z") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("displayname")) ? new XElement(ns + "displayname", Name) : null,
                (!requestedProperties.Any() || requestedProperties.Contains("getlastmodified")) ? new XElement(ns + "getlastmodified", Contents.OrderByDescending(k => k.Version).First().CreatedOn.ToString("r")) : null,
                (!requestedProperties.Any() || requestedProperties.Contains("resourcetype")) ? new XElement(ns + "resourcetype") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("getcontentlength")) ? new XElement(ns + "getcontentlength", Contents.OrderByDescending(k => k.Version).First().RawData.Length) : null,
                (!requestedProperties.Any() || requestedProperties.Contains("getcontenttype")) ? new XElement(ns + "getcontenttype", "application/octet-stream") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("lockdiscovery")) ? new XElement(ns + "lockdiscovery") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("supportedlock")) ? new XElement(ns + "supportedlock") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("ishidden")) ? new XElement(ns + "ishidden", 0) : null
            ),
            new XElement(ns + "status", "HTTP/1.1 200 OK")
        );

    [DontPrivilege]
    public bool UserCan(User user, string priv)
    {
        Guid[] userRoles = user.Roles?.Select(r => r.RoleId).ToArray() ?? Array.Empty<Guid>();

        return (Folder != null && user.IsAdminOfApp(Folder.AppId)) || (Folder?.Roles?.Where(pr => userRoles.Contains(pr.RoleId))
            .SelectMany(pr => pr.Role?.Privileges ?? Array.Empty<string>())
            .Contains(priv) ?? false);
    }
}
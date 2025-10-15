using cCoder.Core.Objects.Attributes;
using cCoder.Core.Objects.Entities.CMS;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace cCoder.Core.Objects.Entities.DMS;

[Table("Folders", Schema = "DMS")]
public class Folder
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [ForeignKey("App")]
    public int AppId { get; set; }

    [ForeignKey("Parent")]
    public Guid? ParentId { get; set; }

    public string Name { get; set; }

    public string Path { get; set; }

    public DateTimeOffset? DeletedOn { get; set; }

    public App App { get; set; }

    public Folder Parent { get; set; }

    public ICollection<Folder> SubFolders { get; set; }

    public ICollection<File> Files { get; set; }

    public ICollection<FolderRole> Roles { get; set; }

    [DontPrivilege]
    public void RecomputePaths()
    {
        string newPath = ParentId != null
            ? $"{Parent.Path}/{Name.Replace(" ", "")}"
            : $"{Name.Replace(" ", "")}";

        if (newPath != Path)
        {
            Path = newPath;
            SubFolders?.ForEach(f => f.RecomputePaths());
        }
    }

    [DontPrivilege]
    public XElement ToWebDavResponse(string urlBase, XNamespace ns, IEnumerable<string> requestedProperties)
    {
        XElement propStat = BuildPropStatResponse(ns, requestedProperties);
        XElement response = new(ns + "response", new XElement(ns + "href", $"{urlBase}Core/App({AppId})/DAV/{Path}"), propStat);
        List<string> unsupportedProps = new() { "getcontentlength", "executable", "checked-in", "checked-out" };

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
                (!requestedProperties.Any() || requestedProperties.Contains("creationdate")) ? new XElement(ns + "creationdate", DateTimeOffset.Now.ToString("s") + "Z") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("displayname")) ? new XElement(ns + "displayname", Name) : null,
                (!requestedProperties.Any() || requestedProperties.Contains("getlastmodified")) ? new XElement(ns + "getlastmodified", DateTimeOffset.Now.ToString("s") + "Z") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("resourcetype")) ? new XElement(ns + "resourcetype", new XElement(ns + "collection")) : null,
                (!requestedProperties.Any() || requestedProperties.Contains("lockdiscovery")) ? new XElement(ns + "lockdiscovery") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("supportedlock")) ? new XElement(ns + "supportedlock") : null,
                (!requestedProperties.Any() || requestedProperties.Contains("isfolder")) ? new XElement(ns + "isfolder", 1) : null,
                (!requestedProperties.Any() || requestedProperties.Contains("ishidden")) ? new XElement(ns + "ishidden", 0) : null
            ),
            new XElement(ns + "status", "HTTP/1.1 200 OK")
        );


    [DontPrivilege]
    public bool UserCan(User user, string priv)
    {
        Guid[] userRoles = user.Roles?.Select(r => r.RoleId).ToArray() ?? Array.Empty<Guid>();
        return user.IsAdminOfApp(AppId) || (Roles?.Where(pr => userRoles.Contains(pr.RoleId))
            .SelectMany(pr => pr.Role?.Privileges ?? Array.Empty<string>())
            .Contains(priv) ?? false);
    }
}
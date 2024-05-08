using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Packaging;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Objects.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;

namespace cCoder.Core.Data;

public partial class CoreDataContext : EFDataContext<User, Role>, ICoreDataContext
{
    private User user = null;

    public override User User
    {
        get
        {
            if (user == null)
            {
                string userName = AuthInfo.SSOUserId ?? "Guest";
                user = GetUserInformation(userName);
            }
            return user;
        }
    }

    private User GetUserInformation(string userName)
    {
        User user = Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefault(u => u.Id == userName);

        if (userName == "Guest")
            user = new User 
            { 
                DefaultCultureId = string.Empty, 
                IsActive = true, 
                Id = "Guest", 
                DisplayName = "Guest", 
                Email = "guest@corporatelinx.com" 
            };

        Role[] roles = Roles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.Users.Any(ur => ur.UserId == user.Id))
            .ToArray();

        user.Roles = roles.Select(r => new UserRole
        {
            UserId = user.Id,
            RoleId = r.Id,
            Role = r
        }).ToArray();

        return user;
    }

    // Packaging
    public virtual DbSet<Package> Packages { get; set; }
    public virtual DbSet<PackageItem> PackageItems { get; set; }

    // Join Entities
    public virtual DbSet<UserRole> UserRoles { get; set; }

    private readonly ILogger log;

    public CoreDataContext(ICoreAuthInfo auth, Config config, ILogger<CoreDataContext> log) 
        : base(auth, config, log)
    {
        EventManager = new CoreEventManager(log, this, config, auth);
        this.log = log;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(Config.ConnectionStrings["Core"]);

        if(Config.LogSQL)
            optionsBuilder.LogTo((message) =>
            {
                if (message.Contains("Executing") || message.Contains("transaction"))
                    System.Diagnostics.Debug.WriteLine(message);
            });
    }

    public override void SetAuth(ICoreAuthInfo auth)
    {
        base.SetAuth(auth);
        user = null;
    }

    public override IQueryable<T> GetAll<T>(bool trackChanges = true)
    {
        IQueryable<T> result = base.GetAll<T>(trackChanges);

        return typeof(T).IsAssignableFrom(typeof(IAmRoleSecured<Role>))
            ? result.Where("!Roles.Any() || Roles.Any(Read && Users.Any(Id == @0))", User.GetId()).Include("Roles")
            : result;
    }

    private sealed class SSOUser
    {
        [Key]
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PhoneNumber { get; set; }

        public User ToCoreUser()
            => new()
            {
                Id = Id,
                IsActive = true,
                DisplayName = DisplayName,
                Email = Email,
                DefaultCultureId = string.Empty
            };
    }
}
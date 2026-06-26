using cCoder.Security.Objects.Entities;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Exposures;

internal static class CoreSecurityApiModelBuilderExtensions
{
    internal static void ConfigureCoreSecurityApiModel(this ODataConventionModelBuilder builder)
    {
        EntityTypeConfiguration<SSOUser> userType = builder.EntityType<SSOUser>();

        userType.Ignore(u => u.PasswordHash);
        userType.Ignore(u => u.AccessFailedCount);
        userType.Ignore(u => u.Tokens);
        userType.Ignore(u => u.LockoutEnabled);
        userType.Ignore(u => u.LockoutEndDateUtc);

        userType.Collection.Function("Me").ReturnsFromEntitySet<SSOUser>("SSOUser");
        userType.Collection.Action("AcceptInvite");

        EntityTypeConfiguration<UserEvent> userEventType = builder.EntityType<UserEvent>();
        userEventType.Ignore(u => u.Session);

        builder.EntitySet<SSOUser>("SSOUser");
        builder.EntitySet<SSORole>("SSORole");
        builder.EntitySet<SSOPrivilege>("SSOPrivilege");
        builder.EntitySet<Tenant>("Tenant");
        builder.EntitySet<TenantAnalysis>("TenantAnalysis");
        builder.EntitySet<UserEvent>("UserEvent");
        builder.EntitySet<SSOUserRole>("SSOUserRole");
        builder.EntityType<SSOUserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
    }
}

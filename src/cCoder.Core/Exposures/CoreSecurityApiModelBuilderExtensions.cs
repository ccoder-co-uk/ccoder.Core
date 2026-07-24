// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.Entities;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Exposures;

internal static class CoreSecurityApiModelBuilderExtensions
{
    internal static void ConfigureCoreSecurityApiModel(this ODataConventionModelBuilder builder)
    {
        EntityTypeConfiguration<SSOUser> userType = builder.EntityType<SSOUser>();

        userType.Ignore(propertyExpression: u => u.PasswordHash);
        userType.Ignore(propertyExpression: u => u.AccessFailedCount);
        userType.Ignore(propertyExpression: u => u.Tokens);
        userType.Ignore(propertyExpression: u => u.LockoutEnabled);
        userType.Ignore(propertyExpression: u => u.LockoutEndDateUtc);

        userType.Collection.Function(name: "Me")
            .ReturnsFromEntitySet<SSOUser>(entitySetName: "SSOUser");

        userType.Collection.Action(name: "AcceptInvite");

        EntityTypeConfiguration<UserEvent> userEventType = builder.EntityType<UserEvent>();
        userEventType.Ignore(propertyExpression: u => u.Session);

        builder.EntitySet<SSOUser>(name: "SSOUser");
        builder.EntitySet<SSORole>(name: "SSORole");
        builder.EntitySet<SSOPrivilege>(name: "SSOPrivilege");
        builder.EntitySet<Tenant>(name: "Tenant");
        builder.EntitySet<TenantAnalysis>(name: "TenantAnalysis");
        builder.EntitySet<UserEvent>(name: "UserEvent");
        builder.EntitySet<SSOUserRole>(name: "SSOUserRole");

        builder.EntityType<SSOUserRole>()
            .HasKey(keyDefinitionExpression: ur => new { ur.UserId, ur.RoleId });
    }
}
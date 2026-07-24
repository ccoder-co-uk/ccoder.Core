// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using cCoder.Security.Objects.Entities;
using Microsoft.Extensions.Options;
using Microsoft.OData.ModelBuilder;

namespace cCoder.Core.Dependencies.OData;

internal sealed class CoreSecurityApiModelDependency
    : IConfigureOptions<ODataConventionModelBuilder>
{
    public void Configure(ODataConventionModelBuilder options)
    {
        EntityTypeConfiguration<SSOUser> userType = options.EntityType<SSOUser>();

        userType.Ignore(propertyExpression: user => user.PasswordHash);
        userType.Ignore(propertyExpression: user => user.AccessFailedCount);
        userType.Ignore(propertyExpression: user => user.Tokens);
        userType.Ignore(propertyExpression: user => user.LockoutEnabled);
        userType.Ignore(propertyExpression: user => user.LockoutEndDateUtc);

        userType.Collection.Function(name: "Me")
            .ReturnsFromEntitySet<SSOUser>(entitySetName: "SSOUser");

        userType.Collection.Action(name: "AcceptInvite");

        EntityTypeConfiguration<UserEvent> userEventType = options.EntityType<UserEvent>();
        userEventType.Ignore(propertyExpression: userEvent => userEvent.Session);

        options.EntitySet<SSOUser>(name: "SSOUser");
        options.EntitySet<SSORole>(name: "SSORole");
        options.EntitySet<SSOPrivilege>(name: "SSOPrivilege");
        options.EntitySet<Tenant>(name: "Tenant");
        options.EntitySet<TenantAnalysis>(name: "TenantAnalysis");
        options.EntitySet<UserEvent>(name: "UserEvent");
        options.EntitySet<SSOUserRole>(name: "SSOUserRole");

        options.EntityType<SSOUserRole>()
            .HasKey(keyDefinitionExpression: userRole => new { userRole.UserId, userRole.RoleId });
    }
}
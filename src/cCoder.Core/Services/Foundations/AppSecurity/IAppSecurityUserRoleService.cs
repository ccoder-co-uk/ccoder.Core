// ---------------------------------------------------------------
// Copyright (c) Paul.Ward@ccoder.co.uk
// ---------------------------------------------------------------

using System.Threading.Tasks;
using cCoder.Data.Models.CMS;

namespace cCoder.Core.Services.Foundations.AppSecurity;

internal interface IAppSecurityUserRoleService
{
    ValueTask SaveAppUserRolesAsync(App app);
}
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace cCoder.Core.Tests.Api;

public sealed class LegacyDocumentManagementRouteRewriteTests
{
    [Theory]
    [InlineData("/Api/Core/Folder", "/Api/DocumentManagement/Folder")]
    [InlineData("/Api/Core/Folder(66b2e10e-8d1c-492b-3b03-08dc359dace5)", "/Api/DocumentManagement/Folder(66b2e10e-8d1c-492b-3b03-08dc359dace5)")]
    [InlineData("/Api/Core/File", "/Api/DocumentManagement/File")]
    [InlineData("/Api/Core/FileContent/123", "/Api/DocumentManagement/FileContent/123")]
    [InlineData("/api/core/folderrole", "/Api/DocumentManagement/FolderRole")]
    public void RewriteLegacyDocumentManagementRoute_ShouldRewriteLegacyCoreDmsPaths(
        string requestPath,
        string expectedPath)
    {
        PathString actual = WebApplicationExtensions.RewriteLegacyDocumentManagementRoute(new PathString(requestPath));

        actual.Value.Should().Be(expectedPath);
    }

    [Theory]
    [InlineData("/Api/Core/App")]
    [InlineData("/Api/Core/FolderPermissions")]
    [InlineData("/Api/DocumentManagement/Folder")]
    [InlineData("/Api/DMS/content/test2")]
    public void RewriteLegacyDocumentManagementRoute_ShouldLeaveNonLegacyDmsPathsUnchanged(string requestPath)
    {
        PathString actual = WebApplicationExtensions.RewriteLegacyDocumentManagementRoute(new PathString(requestPath));

        actual.Value.Should().Be(requestPath);
    }
}

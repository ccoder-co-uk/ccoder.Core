using Microsoft.EntityFrameworkCore;
using System.Text;
using Xunit.Abstractions;

namespace cCoder.Core.E2E.IntegrationTests.DMS.Folder
{
    [Collection("Application")]
    public class FolderMovingScenarios(ApplicationFixture fixture, ITestOutputHelper helper)
    {
        [Fact]
        public async Task CanMoveFolder()
        {
            //given
            var payload = new StringContent("test 123!!", Encoding.UTF8, "text/plain");
            var url = "/api/DMS/test?moveTo=DMS/test2";

            await fixture.Client.PostAsync("/api/DMS/test/test.txt", new StringContent("test 123!!", Encoding.UTF8, "text/plain"));

            //when
            var moveRequest = await fixture.Client.PostAsync(url, null);

            //then
            moveRequest.EnsureSuccessStatusCode();

            var folderPaths = fixture.CoreDataContext
                .GetAll<Objects.Entities.DMS.Folder>()
                .IgnoreQueryFilters()
                .Select(f => f.Path)
                .ToList();

            helper.WriteLine($"Folders in database: {string.Join(",", folderPaths)}");

            Assert.True(folderPaths.Count() == 2);
            Assert.True(folderPaths.Contains("test2"));
            Assert.True(folderPaths.Contains("test2/test"));
            Assert.True(fixture.CoreDataContext.Files.IgnoreQueryFilters().Count(f => f.Name == "test.txt") == 1);
            Assert.True(fixture.CoreDataContext.FileContents.IgnoreQueryFilters().Any());
        }
    }
}

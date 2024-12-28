using cCoder.Core.Objects.Entities.DMS;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace cCoder.Core.E2E.IntegrationTests.DMS.File
{
    [Collection("Application")]
    public class FileCreationScenarios(ApplicationFixture fixture)
    {
        [Fact]
        public async Task CanCreateFileAndIsInDatabase()
        {
            //given
            var payload = new StringContent("test 123!!", Encoding.UTF8, "text/plain");
            var url = "/api/DMS/test/test.txt";

            //when
            var req = await fixture.Client.PostAsync(url, payload);

            //then
            req.EnsureSuccessStatusCode();

            Assert.True(fixture.CoreDataContext.GetAll<Folder>().IgnoreQueryFilters().Count() == 1);
            Assert.True(fixture.CoreDataContext.GetAll<Folder>().IgnoreQueryFilters().Any(u => u.Name.Contains("test")));
            Assert.True(fixture.CoreDataContext.Files.IgnoreQueryFilters().Any(f => f.Name == "test.txt"));
            Assert.True(fixture.CoreDataContext.FileContents.IgnoreQueryFilters().Any());
        }
    }
}
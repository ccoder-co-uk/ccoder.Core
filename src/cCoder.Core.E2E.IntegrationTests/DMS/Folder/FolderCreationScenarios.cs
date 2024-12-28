using Microsoft.EntityFrameworkCore;
using System.Text;

namespace cCoder.Core.E2E.IntegrationTests.DMS.Folder
{
    [Collection("Application")]
    public class FolderCreationScenarios(ApplicationFixture fixture)
    {
        [Fact]
        public async Task CanCreateFolderAndIsInDatabase()
        {
            //given
            var payload = new StringContent("test 123!!", Encoding.UTF8, "text/plain");
            var url = "/api/DMS/test/test.txt";

            //when
            var req = await fixture.Client.PostAsync(url, payload);

            //then
            req.EnsureSuccessStatusCode();

            Assert.True(fixture.CoreDataContext.GetAll<Objects.Entities.DMS.Folder>().IgnoreQueryFilters().Count() == 1);
            Assert.True(fixture.CoreDataContext.GetAll<Objects.Entities.DMS.Folder>().IgnoreQueryFilters().Any(u => u.Name.Contains("test")));
        }
    }
}

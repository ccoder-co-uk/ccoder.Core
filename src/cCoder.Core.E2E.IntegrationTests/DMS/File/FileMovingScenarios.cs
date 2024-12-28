using Microsoft.EntityFrameworkCore;
using System.Text;
using Xunit.Abstractions;

namespace cCoder.Core.E2E.IntegrationTests.DMS.File
{
    [Collection("Application")]
    public class FileMovingScenarios(ApplicationFixture fixture, ITestOutputHelper helper)
    {
        [Fact]
        public async Task CanMoveFileToExistingFile()
        {
            //given
            helper.WriteLine("Creating file at test/test.txt");

            var url = "/api/DMS/test/test.txt?moveTo=test2/test.txt";

            await fixture.Client.PostAsync("/api/DMS/test/test.txt", new StringContent("test 123!!", Encoding.UTF8, "text/plain"));

            Thread.Sleep(1000);

            helper.WriteLine("Creating file at test/test2.txt");
            //Delay here as we expect the 'newest' version to be preserved
            await fixture.Client.PostAsync("/api/DMS/test/test2.txt", new StringContent("test 456!!", Encoding.UTF8, "text/plain"));

            var originalFolder = fixture.CoreDataContext
                .Folders
                .IgnoreQueryFilters()
                .First(f => f.Name.Contains("test"));

            helper.WriteLine($"Original Folder Id: {originalFolder.Id}");

            //when
            var moveRequest = await fixture.Client.PostAsync(url, null);

            //then
            moveRequest.EnsureSuccessStatusCode();

            var getRequest = await fixture.Client.GetAsync("api/DMS/test/test2.txt");

            var fileContent = await getRequest.Content.ReadAsStringAsync();

            helper.WriteLine(fileContent);

            Assert.True(fileContent == "test 456!!");
        }

        [Fact]
        public async Task MoveFileToExistingFile_DeletesOldFile()
        {
            //given
            helper.WriteLine("Creating file at test/test.txt");

            var url = "/api/DMS/test/test.txt?moveTo=test2/test.txt";

            await fixture.Client.PostAsync("/api/DMS/test/test.txt", new StringContent("test 123!!", Encoding.UTF8, "text/plain"));

            var originalFile = fixture.CoreDataContext
                .Files
                .IgnoreQueryFilters()
                .First(f => f.Name == "test.txt");

            helper.WriteLine($"Original File Id: {originalFile.Id}");

            Thread.Sleep(1000);

            helper.WriteLine("Creating file at test2/test.txt");
            //Delay here as we expect the 'newest' version to be preserved
            await fixture.Client.PostAsync("/api/DMS/test2/test.txt", new StringContent("test 456!!", Encoding.UTF8, "text/plain"));

            var originalFolder = fixture.CoreDataContext
                .Folders
                .IgnoreQueryFilters()
                .First(f => f.Name.Contains("test"));

            helper.WriteLine($"Original Folder Id: {originalFolder.Id}");

            //when
            var moveRequest = await fixture.Client.PostAsync(url, null);

            //then
            moveRequest.EnsureSuccessStatusCode();

            Assert.True(!fixture.CoreDataContext.Files.IgnoreQueryFilters().Any(u => u.Id == originalFile.Id));
        }

        [Fact]
        public async Task MoveFileToExistingFile_PreservesFirstFileNewestVersion()
        {
            //given
            helper.WriteLine("Creating file at test/test.txt");

            var url = "/api/DMS/test/test.txt?moveTo=test2/test.txt";

            await fixture.Client.PostAsync("/api/DMS/test/test.txt", new StringContent("test 123!!", Encoding.UTF8, "text/plain"));

            var originalFile = fixture.CoreDataContext
                .Files
                .IgnoreQueryFilters()
                .First(f => f.Name == "test.txt");

            helper.WriteLine($"Original File Id: {originalFile.Id}");

            helper.WriteLine("Creating file at test2/test.txt");
            //Delay here as we expect the 'newest' version to be preserved
            await fixture.Client.PostAsync("/api/DMS/test2/test.txt", new StringContent("test 456!!", Encoding.UTF8, "text/plain"));

            await fixture.Client.PostAsync("/api/DMS/test/test.txt", new StringContent("test 789!!", Encoding.UTF8, "text/plain"));

            var originalFolder = fixture.CoreDataContext
                .Folders
                .IgnoreQueryFilters()
                .First(f => f.Name.Contains("test"));

            helper.WriteLine($"Original Folder Id: {originalFolder.Id}");

            //when
            var moveRequest = await fixture.Client.PostAsync(url, null);

            //then
            moveRequest.EnsureSuccessStatusCode();

            var getRequest = await fixture.Client.GetAsync("api/DMS/test2/test.txt");

            var fileContent = await getRequest.Content.ReadAsStringAsync();

            helper.WriteLine(fileContent);

            Assert.True(fileContent == "test 789!!");
        }

        [Fact]
        public async Task CanMoveFileToNewFolder()
        {
            //given
            var url = "/api/DMS/test/test.txt?moveTo=test2/test.txt";

            await fixture.Client.PostAsync("/api/DMS/test/test.txt", new StringContent("test 123!!", Encoding.UTF8, "text/plain"));

            var originalFolder = fixture.CoreDataContext
                .Folders
                .IgnoreQueryFilters()
                .First(f => f.Name.Contains("test"));

            helper.WriteLine($"Original Folder Id: {originalFolder.Id}");

            //when
            var req = await fixture.Client.PostAsync(url, null);

            //then
            req.EnsureSuccessStatusCode();

            var destinationFolder = fixture.CoreDataContext
                .Folders
                .IgnoreQueryFilters()
                .First(f => f.Name.Contains("test2"));

            helper.WriteLine($"Expected Folder Id: {destinationFolder.Id}");

            var file = fixture.CoreDataContext
                .Files
                .IgnoreQueryFilters()
                .First(f => f.Name == "test.txt");

            helper.WriteLine($"File Path: {file.Path}");
            helper.WriteLine($"File Folder Id : {file.FolderId}");

            Assert.True(file.Path == "test2/test.txt");
            Assert.True(file.FolderId == destinationFolder.Id);
            Assert.True(file.FolderId != originalFolder.Id);
        }
    }
}

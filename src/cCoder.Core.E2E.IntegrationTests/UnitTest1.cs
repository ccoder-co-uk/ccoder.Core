namespace cCoder.Core.E2E.IntegrationTests
{
    [Collection("Application")]
    public class SanityCheck(ApplicationFixture fixture)
    {
        [Fact]
        public async Task SpinsUp()
        {
            var request = await fixture.Client.GetAsync("api/Core/App");

            var content = await request.Content.ReadAsStringAsync();

            Assert.Contains("localhost", content);
        }
    }
}
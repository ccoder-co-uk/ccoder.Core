using cCoder.Core.Objects.Entities.CMS;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace cCoder.Core.Objects.Workflow.Activities.Templating
{
    public class PageBuilder : TemplatingActivity<dynamic>
    {
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public string Layout { get; set; }
        public string ResourceKey { get; set; }
        public bool ShowOnMenus { get; set; }
        public int ParentPageId { get; set; }

        public override async Task Execute()
        {
            using System.Net.Http.HttpClient api = GetHttpClient();
            _ = await api.PostAsJsonAsync("Core/Page", new Page()
            {
                AppId = AppId,
                Layout = Layout,
                ResourceKey = ResourceKey,
                ShowOnMenus = ShowOnMenus,
                PageInfo = new[] { new PageInfo { CultureId = Culture, Description = Description, Keywords = Keywords, Title = Title } },
                Contents = new[] { new Content { CultureId = Culture, Name = "body", Html = await Render(api) } }
            });
        }
    }
}
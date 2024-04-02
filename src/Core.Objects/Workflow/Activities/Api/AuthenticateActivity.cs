using Core.Objects.Extensions;
using System.Threading.Tasks;

namespace Core.Objects.Workflow.Activities.Api
{
    public class AuthenticateActivity : ApiActivity
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public override async Task Execute()
        {
            using System.Net.Http.HttpClient api = GetHttpClient();
            AuthToken = (await api.Authenticate(Username, Password))?.Id;
        }
    }
}
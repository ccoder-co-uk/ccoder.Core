using cCoder.Core.Objects.Entities.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services.Security
{
    public interface IUserService
    {
        Task<User> AddAsync(User newUser);
        Task DeleteAsync(object id);
        Task ForgotPassword(string userEmail, int sourceApp, string token);
        Task<User> UpdateAsync(User entity);
    }
}
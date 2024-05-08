using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Security;
using cCoder.Core.Services;

namespace cCoder.Core.Api.Controllers;

public abstract class CoreJoinEntityOdataController<T, TKeyLeft, TKeyRight> 
    : JoinEntityOdataController<T, User, TKeyLeft, TKeyRight> where T : class, new()
{
    public CoreJoinEntityOdataController(ICoreService<T> service, ICoreAuthInfo auth, ILogger log) 
        : base(service, auth, log) { }
}
using cCoder.DocumentManagement.Models;
using cCoder.Data.Models.CMS;
using cCoder.Data.Models.DMS;
using cCoder.Data.Models.Security;

namespace cCoder.Core.Brokers.DocumentManagement;

public interface IDocumentManagementAppBroker
{
    ValueTask AddAsync(App app);
    ValueTask UpdateAsync(App app);
    ValueTask DeleteAsync(int appId);
}


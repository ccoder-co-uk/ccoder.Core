using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.Mail;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace cCoder.Core.Services.CMS
{
    public class QueuedEmailService : CoreService<QueuedEmail>, IQueuedEmailService
    {
        public QueuedEmailService(ICoreDataContext db) : base(db) { }

        public override Task<QueuedEmail> AddAsync(QueuedEmail entity) =>
            AddAsync(entity, false);

        public async Task<QueuedEmail> AddAsync(QueuedEmail email, bool checkPrivs) =>
            checkPrivs
                ? await base.AddAsync(email)
                : await Db.AddAsync(email);

        public override async Task DeleteAsync(object id)
        {
            int queuedEmailId = (int)id;

            QueuedEmail queuedEmail = GetAll(true)
                .Include(q => q.FailedSends)
                .FirstOrDefault(r => r.Id == queuedEmailId);

            if (queuedEmail == null)
                throw new SecurityException("Access Denied!");

            if (!User.Can(queuedEmail.AppId, "queuedemail_delete"))
                throw new SecurityException("Access Denied!");

            await Db.DeleteAllAsync(queuedEmail.FailedSends);
            await Db.DeleteAsync(queuedEmail);
        }
    }
}
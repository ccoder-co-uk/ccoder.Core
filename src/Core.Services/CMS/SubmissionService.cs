using cCoder.Core.Objects;
using cCoder.Core.Objects.Entities.CMS;

namespace cCoder.Core.Services.CMS
{
    public class SubmissionService : CoreService<Submission>
    {
        public SubmissionService(ICoreDataContext db) : base(db) { }
    }
}
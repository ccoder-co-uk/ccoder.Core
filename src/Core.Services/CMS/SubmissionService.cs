using Core.Objects;
using Core.Objects.Entities.CMS;

namespace Core.Services.CMS
{
    public class SubmissionService : CoreService<Submission>
    {
        public SubmissionService(ICoreDataContext db) : base(db) { }
    }
}
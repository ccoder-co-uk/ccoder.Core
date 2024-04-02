using System;
using System.Threading.Tasks;

namespace Scheduler
{
    public interface IScheduledOperationRunner : IDisposable
    {
        Task Run();
    }
}
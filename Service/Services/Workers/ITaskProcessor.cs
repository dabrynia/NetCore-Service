using System.Threading;
using System.Threading.Tasks;

namespace Service.Services.Workers
{
    public interface ITaskProcessor
    {
        Task RunAsync(int number, CancellationToken token);
    }
}

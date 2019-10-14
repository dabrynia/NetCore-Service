using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services.TaskQueue
{
    public interface IBackgroundTaskQueue
    {
        /// <summary>
        /// Размер очереди
        /// </summary>
        int Size { get; }

        void QueueBackgroundWorlItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}

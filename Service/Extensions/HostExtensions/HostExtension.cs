using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Service.Extensions.HostExtensions
{
    public static class HostExtension
    {
        public static Task RunService(this IHostBuilder hostBuilder)
        {
            return hostBuilder.RunConsoleAsync();
        }
    }
}

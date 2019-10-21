using Microsoft.Extensions.Hosting;
using Service.WindowsServices;
using System;
using System.Threading.Tasks;

namespace Service.Extensions.HostExtensions
{
    public static class WindowsHostExtension
    {
        public static async Task RunService(this IHostBuilder hostBuilder)
        {
            if (!Environment.UserInteractive)
            {
                await hostBuilder.RunAsServiceAsync();
            }
            else
            {
                await hostBuilder.RunConsoleAsync();
            }
        }
    }
}

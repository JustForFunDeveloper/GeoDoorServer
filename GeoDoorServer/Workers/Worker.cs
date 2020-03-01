using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace GeoDoorServer3.Workers
{
    public class Worker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}

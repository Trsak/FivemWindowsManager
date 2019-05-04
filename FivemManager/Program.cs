using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigFile;

namespace FivemManager
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new ConfigReader("config/manager.conf");
            var server = new FivemServer(config);

            var periodTimeSpan = TimeSpan.FromMinutes(config.TryGetValue<int>("CrashCheckInterval"));

            var timer = new System.Threading.Timer((e) => { server.CheckIfCrashed(); }, null, periodTimeSpan,
                periodTimeSpan);


            server.StartServer();

            var restartTimes = config.TryGetValue<string>("RestartTimes");

            var times = restartTimes.Split(';');
            foreach (var time in times)
            {
                server.AddRestartJob(time);
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(server.CancelHandler);

            while (server.IsRunning)
            {
                Console.ReadKey();
            }
        }
    }
}
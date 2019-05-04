using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConfigFile;

namespace FivemManager
{
    internal class FivemServer
    {
        private bool _isRunning;
        private readonly ConfigReader _config;

        public FivemServer(ConfigReader config)
        {
            this._config = config;
        }

        public void StartServer()
        {
            LogToConsole("Starting server...");
            var process = new System.Diagnostics.Process();

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                FileName = this._config.TryGetValue<string>("FXServerLocation") + "/FXServer.exe",
                Arguments = "+set citizen_dir " + this._config.TryGetValue<string>("FXServerLocation") +
                            "/citizen/ +exec " + this._config.TryGetValue<string>("ConfigFile"),
                WorkingDirectory = this._config.TryGetValue<string>("ServerLocation")
            };

            process.StartInfo = startInfo;
            process.Start();

            this._isRunning = true;

            var restartTimes = this._config.TryGetValue<string>("RestartTimes");

            var times = restartTimes.Split(';');
            foreach (var time in times)
            {
                this.AddRestartJob(time);
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);

            while (this._isRunning)
            {
                Console.ReadKey();
            }
        }

        public void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            this._isRunning = false;

            StopServer();

            LogToConsole("Stopping server...");
            Environment.Exit(0);
        }

        protected void AddRestartJob(string dailyTime)
        {
            var timeParts = dailyTime.Split(new char[1] {':'});

            var dateNow = DateTime.Now;
            var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day,
                int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));

            TimeSpan ts;
            if (date > dateNow)
            {
                ts = date - dateNow;
            }
            else
            {
                date = date.AddDays(1);
                ts = date - dateNow;
            }

            LogToConsole("Registered automatic server restart in " + dailyTime);
            Task.Delay(ts).ContinueWith((x) => OnRestart());
        }

        private static void StopServer()
        {
            foreach (var process in Process.GetProcessesByName("FXServer"))
            {
                process.Kill();
            }
        }

        private void OnRestart()
        {
            StopServer();
            LogToConsole("Restarting server...");
            System.Threading.Thread.Sleep(5000);
            this.StartServer();
        }

        private static void LogToConsole(string text)
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + text);
        }
    }
}
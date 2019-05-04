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
            LogToConsole("Zapínám server...");
            var process = new System.Diagnostics.Process();

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                FileName = this._config.TryGetValue<string>("FXServerLocation") + "/FXServer.exe",
                Arguments = "+set citizen_dir " + this._config.TryGetValue<string>("FXServerLocation") +
                            "/citizen/ +exec server.cfg",
                WorkingDirectory = this._config.TryGetValue<string>("ServerLocation")
            };

            process.StartInfo = startInfo;
            process.Start();

            this._isRunning = true;

            this.AddRestartJob("02:00:05");
            this.AddRestartJob("12:00:05");
            this.AddRestartJob("18:00:05");

            Console.CancelKeyPress += new ConsoleCancelEventHandler(cancelHandler);

            while (this._isRunning)
            {
                Console.ReadKey();
            }
        }

        protected void cancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            this._isRunning = false;

            StopServer();

            LogToConsole("Ukončuji server...");
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
            LogToConsole("Restartuji server...");
            System.Threading.Thread.Sleep(5000);
            this.StartServer();
        }

        private static void LogToConsole(string text)
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + text);
        }
    }
}
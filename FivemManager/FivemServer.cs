using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConfigFile;

namespace FivemManager
{
    internal class FivemServer
    {
        public bool IsRunning;
        private bool _isFivemServerRunning;
        private readonly ConfigReader _config;

        public FivemServer(ConfigReader config)
        {
            this._config = config;
            this.IsRunning = false;
            this._isFivemServerRunning = false;
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

            this.IsRunning = true;
            this._isFivemServerRunning = true;
        }

        public void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            this.IsRunning = false;

            StopServer();

            LogToConsole("Stopping server...");
            Environment.Exit(0);
        }

        public void AddRestartJob(string dailyTime)
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

        private void StopServer()
        {
            this._isFivemServerRunning = false;

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

        public void CheckIfCrashed()
        {
            LogToConsole("CRASH CHECK: Checking, if server is running.");

            if (!this._isFivemServerRunning) return;

            var processesNumber = Process.GetProcessesByName("FXServer").GetLength(0);
            if (processesNumber == 0)
            {
                LogToConsole("CRASH CHECK: Server is not running!");
                this.StartServer();
                return;
            }

            LogToConsole("CRASH CHECK: Server is running.");
        }

        private static void LogToConsole(string text)
        {
            Console.WriteLine(DateTime.Now.ToString("[HH:mm:ss] ") + text);
        }
    }
}
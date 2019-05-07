using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private Process _process;

        public FivemServer(ConfigReader config)
        {
            this._config = config;
            this.IsRunning = false;
            this._isFivemServerRunning = false;
            this._process = null;
        }

        public void StartServer()
        {
            LogToConsole("Starting server...");
            this._process = new System.Diagnostics.Process();

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                FileName = this._config.TryGetValue<string>("FXServerLocation") + "/FXServer.exe",
                Arguments = "+set citizen_dir " + this._config.TryGetValue<string>("FXServerLocation") +
                            "/citizen/ +exec " + this._config.TryGetValue<string>("ConfigFile"),
                WorkingDirectory = this._config.TryGetValue<string>("ServerLocation")
            };

            this._process.StartInfo = startInfo;
            this._process.Start();

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

            this._process.Kill();
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
            if (!this._isFivemServerRunning) return;

            LogToConsole("CRASH CHECK: Checking, if server is running.");


            if (!this.IsProcessRunning())
            {
                LogToConsole("CRASH CHECK: Server is not running!");
                this.StartServer();
                return;
            }

            LogToConsole("CRASH CHECK: Server is running."); 
        }

        private static void LogToConsole(string text)
        {
            text = DateTime.Now.ToString("[HH:mm:ss] ") + text;
            Console.WriteLine(text);
            System.IO.File.AppendAllText(@"log.txt", text + "\n");
        }

        private bool IsProcessRunning()
        {
            if (this._process == null)
            {
                return false;
            }

            try
            {
                Process.GetProcessById(this._process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}
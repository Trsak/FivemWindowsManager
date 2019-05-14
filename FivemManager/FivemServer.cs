using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        string date;

        public FivemServer(ConfigReader config)
        {
            _config = config;
            IsRunning = false;
            _isFivemServerRunning = false;
            _process = null;
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Logs/";
            if (!Directory.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Logs/"))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void StartServer()
        {
            LogToConsole("Starting server...");
            _process = new Process();

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = _config.TryGetValue<string>("FXServerLocation") + "/FXServer.exe",
                Arguments = "+set citizen_dir " + _config.TryGetValue<string>("FXServerLocation") +
                            "/citizen/ +exec " + _config.TryGetValue<string>("ConfigFile"),
                WorkingDirectory = _config.TryGetValue<string>("ServerLocation")
            };
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;

            _process.StartInfo = startInfo;
            date = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
            _process.OutputDataReceived += CaptureOutput;
            _process.Start();

            _process.BeginOutputReadLine();
            IsRunning = true;
            _isFivemServerRunning = true;
        }

        public void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            IsRunning = false;

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
            _isFivemServerRunning = false;

            _process.Kill();
        }

        private void OnRestart()
        {
            StopServer();
            LogToConsole("Restarting server...");
            Thread.Sleep(5000);
            StartServer();
        }

        public void CheckIfCrashed()
        {
            if (!_isFivemServerRunning) return;

            LogToConsole("CRASH CHECK: Checking, if server is running.");


            if (!IsProcessRunning())
            {
                LogToConsole("CRASH CHECK: Server is not running!");
                StartServer();
                return;
            }

            LogToConsole("CRASH CHECK: Server is running.");
        }

        private static void LogToConsole(string text)
        {
            text = DateTime.Now.ToString("[HH:mm:ss] ") + text;
            Console.WriteLine(text);
            File.AppendAllText(@"log.txt", text + "\n");
        }

        private void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            if(e.Data != null)
            {
                Console.WriteLine(e.Data);
                File.AppendAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Logs/"+ date + ".txt", e.Data.ToString() + "\n");
            }
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
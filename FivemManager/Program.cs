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
            server.StartServer();
        }
    }
}
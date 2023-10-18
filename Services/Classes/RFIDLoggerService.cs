using DisantAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DisantAPI.Services.Classes
{
    public class RFIDLoggerService : IRFIDLoggerService
    {
        private readonly string _path;
        public RFIDLoggerService(string path) {
            if (Directory.Exists(path))
            { 
                _path = path;
            }
            else
            {
                _path = "";
            }
            Console.WriteLine(path);
            throw new Exception(path);
        }

        public void Log(string message)
        {
            Console.WriteLine(_path);
        }
    }
}

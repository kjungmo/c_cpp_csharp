using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LocalizationTesterD.Tools
{
    public class LogManager
    {
        private string _path;


        #region Constructors
        public LogManager(string path)
        {
            _path = path;
            _SetLogPath();
        }

        public LogManager()
            :this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log"))
        {

        }
        #endregion


        #region Methods 
        private void _SetLogPath()
        {
            if (Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            string logFile = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            _path = Path.Combine(_path, logFile);

        }

        public void Write(string data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_path, true)) // true means to append, not overwrites existing files
                {
                    writer.Write(data); // Write method from StreamWriter Class
                }
            }
            catch (Exception ex)
            { }
        }

        public void WriteLine(string data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_path, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss/t") + data);  // writeLine method from StreamWriter class
                }
            }
            catch (Exception ex)
            { }
        }
        #endregion
    }
}

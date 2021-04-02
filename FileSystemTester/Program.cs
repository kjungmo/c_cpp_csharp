using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileSystemTester
{
    class Program
    {
        static void Main(string[] args)
        {

            string rootPath = @"D:\ZipTest\";

            string[] dirs = Directory.GetDirectories(rootPath);

            foreach (string dir in dirs)
            {
                Console.WriteLine(dir);
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                foreach (FileInfo file in dirInfo.GetFiles("*.txt"))
                {
                    Console.WriteLine(file.FullName);
                    file.Delete();
                }
            }
            
            //DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
            //foreach (FileInfo file in directoryInfo.GetFiles("*.txt") )
            //{
            //    file.Delete();
            //}
            Console.ReadLine();
        }
    }
}

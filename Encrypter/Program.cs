using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Encrypter
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine(LicenseKeyGenerator.KeyGenerator());
            Console.ReadKey();
        }
    }
}

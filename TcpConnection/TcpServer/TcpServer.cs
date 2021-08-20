using CommonUtils;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TcpConnection
{
    class TcpServer
    {
        private static readonly string _deviceName = ConfigurationManager.AppSettings["DeviceName"];
        private static readonly string _startPosition = ConfigurationManager.AppSettings["StartPos"];
        private static MelsecPLCTransferer _plc = new MelsecPLCTransferer();
        static void Main()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["Port"], out int port))
            {
                return;
            }
            if (port < 1024 || 49151 < port)
            {
                return;
            }

            if (!_plc.Init())
            {
                return;
            }

            TcpListener server = new TcpListener(IPAddress.Any, port);
            try
            {
                server.Start();
            }
            catch (SocketException)
            {
                return;
            }

            Console.WriteLine("Server started"); //  ****************

            NetworkStream ns = null;
            StreamReader sr = null;
            StreamWriter sw = null;
            TcpClient client = null;

            while (true)
            {
                try
                {
                    client = server.AcceptTcpClient();
                    Console.WriteLine("Waiting for a connection..."); //  ****************
                    ns = client.GetStream();
                    sr = new StreamReader(ns);
                    sw = new StreamWriter(ns); //  **************************
                    Console.WriteLine("Client connected!"); //  ****************
                    string welcome = "Server Connnect Success!";  //  ****************
                    sw.WriteLine(welcome); //  ****************
                    sw.Flush(); //  ****************
                    while (true)
                    {
                        ReadStreamWriteToPlc(sr);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    ns?.Close();
                    sr?.Close();
                    sw?.Close();
                    client?.Close();
                }
            }
        }

        private static void ReadStreamWriteToPlc(StreamReader reader)
        {
            string fromStream = reader.ReadLine();
            if (!string.IsNullOrEmpty(fromStream) && int.TryParse(fromStream, out int data))
            {
                if (_plc.WriteDeviceBlock(_deviceName, _startPosition, new int[] { data }) == 0)
                {
                    Console.WriteLine($"Writing : [{data}]");
                }
                else
                {
                    Console.WriteLine("Failed writing. PLC disconnected.");
                }
            }
        }
    }
}

using CommonUtils;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CoPick
{
    class MelsecPLCDataReceiver
    {
        private static readonly string _deviceName = ConfigurationManager.AppSettings["DeviceName"];
        private static readonly string _startPosition = ConfigurationManager.AppSettings["StartPos"];
        private static MelsecPLCTransferer _plc = new MelsecPLCTransferer();
        private static int _connectionCounter;
        static void Main()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["KeepaliveTime"], out int keepaliveTime))
            {
                return;
            }

            if (!int.TryParse(ConfigurationManager.AppSettings["KeepaliveInterval"], out int keepaliveInterval))
            {
                return;
            }

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

            Console.WriteLine("Server started");

            NetworkStream ns = null;
            StreamReader sr = null;
            StreamWriter sw = null;
            TcpClient client = null;

            int size = sizeof(UInt32);
            UInt32 on = 1;
            byte[] inArray = new byte[size * 3];
            Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
            Array.Copy(BitConverter.GetBytes((uint)keepaliveTime), 0, inArray, size, size);
            Array.Copy(BitConverter.GetBytes((uint)keepaliveInterval), 0, inArray, size * 2, size);

            while (true)
            {
                try
                {
                    Console.WriteLine($"Waiting for a connection...From {DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture)}");
                    client = server.AcceptTcpClient();
                    Console.WriteLine($"Connected At {DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture)}");
                    client.Client.IOControl(IOControlCode.KeepAliveValues, inArray, null);
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    ns = client.GetStream();
                    sr = new StreamReader(ns);
                    sw = new StreamWriter(ns);
                    Console.WriteLine("Client connected!");
                    string welcome = "Server Connnect Success!";
                    sw.WriteLine(welcome);
                    sw.Flush();
                    _connectionCounter++;
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
                    Console.WriteLine($"Ended at {DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture)}");
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
                    Console.WriteLine($"[{_connectionCounter}]Writing : [{data}] at {DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture)}");
                }
                else
                {
                    Console.WriteLine("Failed writing. PLC disconnected.");
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }
}
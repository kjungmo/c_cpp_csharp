using CommonUtils;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CoPick
{
    class MelsecPLCDataSender
    {
        private static readonly string _deviceName = ConfigurationManager.AppSettings["DeviceName"];
        private static readonly string _startPosition = ConfigurationManager.AppSettings["StartPos"];
        private static readonly string _ipAddressLow = ConfigurationManager.AppSettings["IPAddressLow"];
        private static readonly string _ipAddressSpare = ConfigurationManager.AppSettings["IPAddressSpare"];
        private static int _plcInterval;
        private static int _connectionCounter;
        private static MelsecPLCTransferer _plc = new MelsecPLCTransferer();
        private static bool _ipAddressSwitch;
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

            if (!int.TryParse(ConfigurationManager.AppSettings["ConnectionTimeout"], out int connectionTimeout))
            {
                return;
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["PlcInterval"], out _plcInterval))
            {
                return;
            }

            if (!IPAddress.TryParse(_ipAddressLow, out IPAddress _))
            {
                return;
            }
            if (!IPAddress.TryParse(_ipAddressSpare, out IPAddress _))
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

            NetworkStream ns = null;
            StreamReader sr = null;
            StreamWriter sw = null;

            string ipaddress;

            int size = sizeof(UInt32);
            UInt32 on = 1;
            byte[] inArray = new byte[size * 3];
            Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
            Array.Copy(BitConverter.GetBytes((uint)keepaliveTime), 0, inArray, size, size);
            Array.Copy(BitConverter.GetBytes((uint)keepaliveInterval), 0, inArray, size * 2, size);

            while (true)
            {
                TcpClient client = null;
                try
                {
                    if (_ipAddressSwitch)
                    {
                        ipaddress = _ipAddressLow;
                    }
                    else
                    {
                        ipaddress = _ipAddressSpare;
                    }
                    client = ConnectionTimeoutSocket.Connect(ipaddress, port, connectionTimeout);
                    Console.WriteLine($"Connected{ipaddress} At {DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture)}");

                    client.Client.IOControl(IOControlCode.KeepAliveValues, inArray, null);
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                    ns = client.GetStream();
                    sr = new StreamReader(ns);
                    sw = new StreamWriter(ns);
                    Console.WriteLine(sr.ReadLine());
                    _connectionCounter++;
                    while (true)
                    {
                        ReadPlcWriteToStream(sw);
                        System.Threading.Thread.Sleep(_plcInterval);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    sr?.Close();
                    sw?.Close();
                    ns?.Close();
                    client?.Close();
                    _ipAddressSwitch = !_ipAddressSwitch;
                }
            }
        }

        private static void ReadPlcWriteToStream(StreamWriter writer)
        {
            (int ret, int[] resArray) = _plc.ReadDeviceBlock(_deviceName, _startPosition, 1);
            if (ret == 0)
            {
                writer.WriteLine(resArray[0]);
                writer.Flush();
                Console.WriteLine($"[{_connectionCounter}]Sending [{resArray[0]}] At {DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture)}");
            }
            else
            {
                Console.WriteLine("Check for PLC connection.");
            }
        }
    }
}

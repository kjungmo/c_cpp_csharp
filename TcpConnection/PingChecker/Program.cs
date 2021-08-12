using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace PingChecker
{
    class Program
    {
        public static void Main()
        { 
            try 
            { 
                string ip = "192.168.0.12"; 
                int port = 8240; 
                if (PingTest(ip)) 
                { 
                    Console.WriteLine("{0} PING OK", ip); 
                    if (ConnectTest(ip, port)) 
                    { 
                        Console.WriteLine("{0}:{1} is open.", ip, port); 
                    } 
                    else 
                    { 
                        Console.WriteLine("{0}:{1} is closed.", ip, port); 
                    } 
                } 
                else 
                { 
                    Console.WriteLine("{0} PING NG", ip);
                } 
            }
            catch (Exception e) 
            { 
                Console.WriteLine(e.Message); 
            } 
        }
        private static bool PingTest(string ip)
        {
            bool result = false;
            try
            { 
                Ping pp = new Ping(); 
                PingOptions po = new PingOptions(); 
                po.DontFragment = true; 
                byte[] buf = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaa"); 
                PingReply reply = pp.Send(IPAddress.Parse(ip), 10, buf, po); 
                if (reply.Status == IPStatus.Success) 
                { 
                    result = true; 
                } 
                else 
                { 
                    result = false; 
                } 
                return result; 
            }
            catch 
            { 
                throw; 
            }
        }
        private static bool ConnectTest(string ip, int port) 
        { 
            bool result = false; 
            Socket socket = null; 
            try 
            { 
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false); 
                IAsyncResult ret = socket.BeginConnect(ip, port, null, null); 
                result = ret.AsyncWaitHandle.WaitOne(100, true); 
            } 
            catch 
            { 
            } 
            finally 
            { 
                if (socket != null) 
                { 
                    socket.Close(); 
                } 
            } 
            return result; 
        }

    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    class Server
    {
        static void Main(string[] args)
        {

            string strMsg;
            Int32 port = Convert.ToInt32(ConfigurationManager.AppSettings["PORT"]);
            int timeout = Convert.ToInt32(ConfigurationManager.AppSettings["RECEIVETIMEOUT"]);
            TcpListener sockServer = new TcpListener(IPAddress.Any, port);
            sockServer.Start();
            while (true)
            {
                Console.WriteLine("Server 시작! Client 연결 대기중...");
                TcpClient client = sockServer.AcceptTcpClient();
                client.ReceiveTimeout = timeout;
                Console.WriteLine("Client 접속성공!");

                NetworkStream ns = client.GetStream();
                
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);
                try
                {
                    string welcome = "Server Connnect Success!";
                    sw.WriteLine(welcome);
                    sw.Flush();

                    while (true)
                    {
                        strMsg = sr.ReadLine();
                        if (strMsg == "exit")
                        {
                            break;
                        }
                        Console.WriteLine(strMsg);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    sw.Close();
                    sr.Close();
                    ns.Close();
                    client.Close();
                    Console.WriteLine("Client 연결 종료!");
                }
            }
        }
    }
}

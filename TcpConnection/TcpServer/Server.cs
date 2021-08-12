using System;
using System.Collections.Generic;
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
            try
            {
                string strMsg;
                
                Int32 port = 8240;
                TcpListener sockServer = new TcpListener(IPAddress.Any, port); //IP, Port
                sockServer.Start();

                while (true)
                {
                    Console.WriteLine("Server 시작! Client 연결 대기중...");
                    TcpClient client = sockServer.AcceptTcpClient();//Accept
                    Console.WriteLine("Client 접속성공!");

                    NetworkStream ns = client.GetStream();
                    StreamReader sr = new StreamReader(ns);
                    StreamWriter sw = new StreamWriter(ns);

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

                    sw.Close();
                    sr.Close();
                    ns.Close();
                    client.Close();
                    Console.WriteLine("Client 연결 종료!");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

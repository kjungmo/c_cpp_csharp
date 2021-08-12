using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace TcpConnection
{
    class Program
    {
        public static bool TimeoutChecker { get; set; } = true;
        static void Main(string[] args)
        {
            Timer timer = new Timer();
            timer.Interval = 10;
            timer.Elapsed += new ElapsedEventHandler(ElapsedTime);
            try
            {
                string strRecvMsg;
                string strSendMsg;

                string ipconfigIP = "192.168.0.12";

                TcpClient sockClient = new TcpClient(ipconfigIP, 8240); //소켓생성,커넥트
                NetworkStream networkstream = sockClient.GetStream();
                StreamReader sr = new StreamReader(networkstream);
                StreamWriter sw = new StreamWriter(networkstream);

                strRecvMsg = sr.ReadLine();         //서버로부터 접속 성공 메시지 수신
                Console.WriteLine(strRecvMsg);
                
                timer.Start();

                while (TimeoutChecker)
                {
                    strSendMsg = "Sending Sequential Number";
                    sw.WriteLine(strSendMsg);
                    sw.Flush();
                }

                sw.WriteLine("exit");
                sw.Flush();

                sr.Close();
                sw.Close();
                networkstream.Close();
                sockClient.Close();

                Console.WriteLine("접속 종료!");
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ElapsedTime(object sender, ElapsedEventArgs e)
        {
            TimeoutChecker = false;

        }
    }
}

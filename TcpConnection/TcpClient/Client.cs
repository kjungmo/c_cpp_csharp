using System;
using System.Collections.Generic;
using System.Configuration;
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
            timer.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["SENDTIME"]); ;
            timer.Elapsed += new ElapsedEventHandler(ElapsedTime);

            Int32 port = Convert.ToInt32(ConfigurationManager.AppSettings["PORT"]);
            string ipAddress = ConfigurationManager.AppSettings["IPADDRESS"];
            
            try
            {
                string strRecvMsg;
                string strSendMsg;

                TcpClient sockClient = new TcpClient(ipAddress, port);
                NetworkStream networkstream = sockClient.GetStream();
                StreamReader sr = new StreamReader(networkstream);
                StreamWriter sw = new StreamWriter(networkstream);

                strRecvMsg = sr.ReadLine();         //서버로부터 접속 성공 메시지 수신
                Console.WriteLine(strRecvMsg);
                
                timer.Start();

                while (TimeoutChecker)
                {
                    System.Threading.Thread.Sleep(2000);
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
            catch (Exception e)
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

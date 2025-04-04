﻿using CommonUtils;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CoPick;

namespace TcpConnection
{
    class Client
    {
        private static readonly string _deviceName = ConfigurationManager.AppSettings["DeviceName"];
        private static readonly string _startPosition = ConfigurationManager.AppSettings["StartPos"];
        private static readonly string _ipAddress = ConfigurationManager.AppSettings["IPAddress"];
        private static MelsecPLCTransferer _plc = new MelsecPLCTransferer();

        static void Main()
        {
            if (!IPAddress.TryParse(_ipAddress, out _))
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

            if (!int.TryParse(ConfigurationManager.AppSettings["SendTimeout"], out int sendTimeout))
            {
                return;
            }
            if (!uint.TryParse(ConfigurationManager.AppSettings["KeepAliveInterval"], out uint keepAliveInterval))
            {
                return;
            }
            if (!uint.TryParse(ConfigurationManager.AppSettings["RetryInterval"], out uint retryInterval))
            {
                return;
            }

            if (!_plc.Init())
            {
                return;
            }

            CoPickTcpSocket tcpSocket = new CoPickTcpSocket(_ipAddress, port, keepAliveInterval, retryInterval);

            tcpSocket.Connect(sendTimeout);


            //byte[] buff = new byte[4];
            //TcpClient client = TimeOutSocket.Connect(_ipAddress, port, 1000);
            //NetworkStream ns = client.GetStream();

            //while (true)
            //{
            //    ns.Write(buff, 0, buff.Length);
            //}


            //CoPickTcpSocket tcpSocket = new CoPickTcpSocket(_ipAddress, port, keepAliveInterval, retryInterval);
            //while (true)
            //{
            //    System.Threading.Thread.Sleep(20);
            //    Console.WriteLine($"Connection trial Starts : {DateTime.Now.ToString("HH:mm:ss:fff")}");
            //    tcpSocket.Connect();
            //    Console.WriteLine($"Connection Success : {DateTime.Now.ToString("HH:mm:ss:fff")}");
            //    tcpSocket.SendTimeOut = sendTimeout;
            //    while (tcpSocket.IsConnected)
            //    {
            //        byte[] buff = new byte[4];
            //        System.Threading.Thread.Sleep(1000);
            //        Console.WriteLine("Reading...");
            //        Console.WriteLine($"Write buff : {DateTime.Now.ToString("HH:mm:ss:fff")}");
            //        tcpSocket.Write(buff);
            //        Console.WriteLine(tcpSocket.IsConnected);
            //    }
            //    tcpSocket.Close();
            //    Console.WriteLine($"Connection ended : {DateTime.Now.ToString("HH:mm:ss:fff")}");
            //}
            


            //TcpClient client = null;
            //NetworkStream ns = null;
            //StreamReader sr = null;
            //StreamWriter sw = null;

            //while (true)
            //{
            //    try
            //    {
            //client = new TcpClient(_ipAddress, port);
            //ns = client.GetStream();
            //sr = new StreamReader(ns); // **********
            //sw = new StreamWriter(ns);
            //Console.WriteLine(sr.ReadLine()); // **********
            //while (true)
            //{
            //    ReadPlcWriteToStream(sw);
            //}
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
            //finally
            //{
            //sr?.Close();
            //sw?.Close();
            //ns?.Close();
            //client?.Close();
            //}
            //}
        }

        //private static void ReadPlcWriteToStream(StreamWriter writer)
        //{
        //    (int ret, int[] resArray) = _plc.ReadDeviceBlock(_deviceName, _startPosition, 1);
        //    if (ret == 0)
        //    {
        //        System.Threading.Thread.Sleep(100);
        //        writer.WriteLine(resArray[0]);
        //        writer.Flush();
        //        Console.WriteLine(resArray[0]);
        //    }
        //    else
        //    {
        //        Console.WriteLine("Check for PLC connection.");
        //    }
        //}
    }
}

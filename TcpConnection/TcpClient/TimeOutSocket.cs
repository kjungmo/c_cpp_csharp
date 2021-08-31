using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoPick
{
    class TimeOutSocket
    {
        private static bool _isConnectionSuccessful = false;
        private static Exception socketException;
        private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        public static TcpClient Connect(string ipAddress, int port, int timeoutMilSec)
        {
            TimeoutObject.Reset();
            socketException = null;

            TcpClient client = new TcpClient();
            client.BeginConnect(ipAddress, port, new AsyncCallback(CallBackMethod), client);
            if (TimeoutObject.WaitOne(timeoutMilSec, false))
            {
                if (_isConnectionSuccessful)
                {
                    return client;
                }
                else
                {
                    throw socketException;
                }
            }
            else
            {
                client.Close();
                throw new TimeoutException("Timeout Exception");
            }
        }

        private static void CallBackMethod(IAsyncResult asyncResult)
        {
            try
            {
                _isConnectionSuccessful = false;
                TcpClient client = asyncResult.AsyncState as TcpClient;

                if (client.Client != null)
                {
                    client.EndConnect(asyncResult);
                    _isConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                _isConnectionSuccessful = false;
                socketException = ex;
            }
            finally
            {
                TimeoutObject.Set();
            }
        }
    }
}

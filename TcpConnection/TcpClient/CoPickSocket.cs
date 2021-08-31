using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoPick
{
    public class CoPickTcpSocket : IDisposable
    {
        Socket _socket;
        string _host;
        int _port;
        uint _keepAliveInterval;
        uint _retryInterval;
        bool _connected;
        #region [[[[[[[ TimeoutSocket ]]]]]]]]
        private bool _isConnectionSuccessful = false;
        private Exception socketException;
        private ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        #endregion


        public bool IsConnected { get { return _connected; } }

        public CoPickTcpSocket(string host, int port, uint keepAliveInterval, uint retryInterval)
        {
            _host = host;
            _port = port;
            _keepAliveInterval = keepAliveInterval;
            _retryInterval = retryInterval;
        }

        public int SendTimeOut
        {
            get
            {
                if (_connected == false)
                {
                    return Timeout.Infinite;
                }
                return (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout); 
            }
            set
            {
                if (_connected == false)
                {
                    return;
                }

                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value); 
            }
        }

        public int ReceiveTimeout
        {
            get
            {
                if (_connected == false)
                {
                    return Timeout.Infinite;
                }

                return (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout); 
            }
            set
            {
                if (_connected == false)
                {
                    return;
                } 
                
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value); 
            }
        }

        public bool Connect(int timeoutMilSec)
        {
            TimeoutObject.Reset();
            socketException = null;

            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //_socket.Connect(_host, _port);
                _socket.BeginConnect(_host, _port, new AsyncCallback(CallBackMethod), _socket);

                if (TimeoutObject.WaitOne(timeoutMilSec, false))
                {
                    if (_isConnectionSuccessful)
                    {

                    }
                    else
                    {
                        throw socketException;
                    }
                }
                else
                {
                    _socket.Close();
                    throw new TimeoutException("Connection Trial Timeout Exception");
                }
                _connected = true;

                OnConnected();
                return true;
            }
            catch
            {
                Close();
            }

            return false;
        }

        private void CallBackMethod(IAsyncResult asyncResult)
        {
            try
            {
                _isConnectionSuccessful = false;
                Socket socket = asyncResult.AsyncState as Socket;
                if (socket != null)
                {
                    socket.EndConnect(asyncResult);
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

        public void Close()
        {
            bool fireLostEvent = _connected == true;
            _connected = false;
            if (_socket != null)
            {
                try
                {
                    _socket.Close();
                }
                catch
                {
                    if (fireLostEvent == true)
                    {
                        OnConnectionLost();
                    }

                    _socket = null;
                }
            }
        }

        public event Action Connected;
        protected virtual void OnConnected()
        {
            Debug.WriteLine("Connected");
            if (Connected == null)
            {
                return;
            }

            Connected();
        }

        public event Action ConnectionLost;
        protected virtual void OnConnectionLost()
        {
            Debug.WriteLine("Connection Lost");
            if (ConnectionLost == null)
            {
                return;
            }

            ConnectionLost();
        }
        public int Write(byte[] buffer)
        {
            if (_connected == false)
            {
                return 0;
            }

            int offset = 0;
            int size = buffer.Length;

            int totalSent = 0;

            while (true)
            {
                int sent;

                try
                {
                    sent = _socket.Send(buffer, 0, size, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        break;
                    }
                    Close();
                    break;
                }

                totalSent += sent;

                if (totalSent == buffer.Length)
                {
                    break;
                }

                offset += sent;
                size -= sent;
            }

            return totalSent;
        }

        public int Read(byte[] readBuf)
        {
            return Read(readBuf, 0, readBuf.Length);
        }

        public int Read(byte[] buffer, int offset, int size)
        {
            if (_connected == false)
            {
                return 0;
            }

            if (size == 0)
            {
                return 0;
            }

            int totalRead = 0;
            int readRemains = size;

            while (true)
            {
                int readLen = 0;

                try
                {
                    readLen = _socket.Receive(buffer, offset, readRemains, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        break;
                    }
                }

                if (readLen == 0)
                {
                    Close();
                    break;
                }

                totalRead += readLen;

                if (totalRead == size)
                {
                    break;
                }

                offset += readLen;
                readRemains -= readLen;
            }

            return totalRead;
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
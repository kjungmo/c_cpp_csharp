using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnection
{
    public class CoPickTcpSocket : IDisposable
    {
        Socket _socket;
        readonly string _host;
        readonly int _port;
        readonly uint _keepAliveInterval;
        readonly uint _retryInterval;
        bool _connected;
        public bool IsConnected { get { return _connected; } }

        public CoPickTcpSocket(string host, int port, uint keepAliveInterval, uint retryInterval)
        {
            _host = host;
            _port = port;
            _keepAliveInterval = keepAliveInterval;
            _retryInterval = retryInterval;
        }

        public bool Connect()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(_host, _port);

                int size = sizeof(UInt32);
                UInt32 on = 1;
                UInt32 keepAliveInterval = _keepAliveInterval;
                UInt32 retryInterval = _retryInterval;
                byte[] inArray = new byte[size * 3];
                Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
                Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
                Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);

                _socket.IOControl(IOControlCode.KeepAliveValues, inArray, null);

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

        public int SendTimeOut
        {
            get
            { return (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout); }
            set
            { _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value); }
        }

        public int ReceiveTimeout
        {
            get
            { return (int)_socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout); }
            set
            { _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value); }
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
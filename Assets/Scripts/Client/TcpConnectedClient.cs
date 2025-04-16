using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Client
{
    public class TcpConnectedClient
    {
        private TcpClient _client;
        private Queue<byte[]> _dataReceived;
        private byte[] _readBuffer = new byte[5000];
        private object _readHandler = new object();
        
        private NetworkStream NetworkStream => _client?.GetStream();
        
        public TcpConnectedClient(TcpClient client)
        {
            _client = client;
            
            //if(TcpNetworkManager.Instance.IsServer)
            //    NetworkStream.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);
        }

        public void SendData(byte[] data)
        {
            NetworkStream.Write(data, 0, data.Length);
        }

        public void FlushReceivedData()
        {
            lock (_readHandler)
            {
                while (_dataReceived.Count > 0)
                {
                    var data = _dataReceived.Dequeue();
                    //TcpNetworkManager.Instance.ReceiveData(data);
                }
            }
        }

        public void OnEndConnection(IAsyncResult asyncResult)
        {
            _client.EndConnect(asyncResult);
            NetworkStream.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);
        }

        public void CloseClient()
        {
            _client.Close();
        }

        private void OnRead(IAsyncResult asyncResult)
        {
            if (NetworkStream?.EndRead(asyncResult) == 0)
            {
                //TcpNetworkManager.Instance.DisconnectClient(this);
                return;
            }
            
            lock (_readHandler)
            {
                var data = _readBuffer.TakeWhile(b => (char) b != '\0').ToArray();
                _dataReceived.Enqueue(data);
                
                Array.Clear(_readBuffer, 0, _readBuffer.Length);
            }
            
            NetworkStream?.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);
        }
    }
}

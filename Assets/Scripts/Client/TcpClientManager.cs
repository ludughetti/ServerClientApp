using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Server;
using UnityEngine;

namespace Client
{
    public class TcpClientManager
    {
        private TcpClient _client;
        private Queue<byte[]> _dataReceived = new ();
        
        public byte[] ReadBuffer = new byte[5000];
        public object ReadHandler = new ();
        public NetworkStream NetworkStream => _client?.GetStream();

        public Action<byte[]> OnMessageReceived;
        
        public TcpClientManager(TcpClient client)
        {
            _client = client;
        }

        public void StartClient(IPAddress serverIPAddress, int port)
        {
            Debug.Log($"Starting client with IP {serverIPAddress}:{port}");
            _client.BeginConnect(serverIPAddress, port, OnConnectToServer, null);
        }
        
        public void CloseClient()
        {
            NetworkStream?.Close();
            _client?.Close();
        }
        
        public void SendDataToServer(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            NetworkStream.Write(data, 0, data.Length);
        }

        private void OnConnectToServer(IAsyncResult result)
        {
            Debug.Log($"OnConnectToServer Is NetworkStream available? {NetworkStream != null}");
            _client.EndConnect(result);
            NetworkStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnRead, null);
        }
        
        private void OnRead(IAsyncResult asyncResult)
        {
            Debug.Log("Message received");
            var bytesRead = NetworkStream.EndRead(asyncResult);

            if (bytesRead <= 0)
            {
                TcpServerManager.Instance.DisconnectClient(this);
                return;
            }

            lock (ReadHandler)
            {
                var dataToBroadcast = ReadBuffer.TakeWhile(b => (char) b != '\0').ToArray();
                Debug.Log($"Message enqueued: {Encoding.UTF8.GetString(dataToBroadcast)}");
                _dataReceived.Enqueue(dataToBroadcast);
            }
            
            Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
            NetworkStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnRead, null);
            Debug.Log("Message queued, buffer cleared and listening again");
        }

        public void FlushQueuedMessages()
        {
            Debug.Log("FlushQueuedMessages triggered");
            lock (ReadHandler)
            {
                Debug.Log("Processing queued messages");
                while (_dataReceived.Count > 0)
                {
                    Debug.Log("_dataReceived contains messages");
                    var data = _dataReceived.Dequeue();
                    OnMessageReceived.Invoke(data);
                }
            }
        }
    }
}

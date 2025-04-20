using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Server;
using UnityEngine;
using static Utils.Encoder;

namespace Client
{
    public class TcpClientManager
    {
        private TcpClient _client;
        private Queue<byte[]> _dataReceived = new ();
        
        public byte[] ReadBuffer = new byte[5000];
        public object ReadHandler = new ();
        public NetworkStream NetworkStream => _client?.GetStream();

        public Action<byte[]> OnClientMessageReceived;
        
        public TcpClientManager(TcpClient client)
        {
            _client = client;
        }

        public void StartClient(IPAddress serverIPAddress, int port)
        {
            Debug.Log($"Starting client... connecting to {serverIPAddress}:{port}");
            _client.BeginConnect(serverIPAddress, port, OnConnectToServer, null);
        }
        
        public void CloseClient()
        {
            NetworkStream?.Close();
            _client?.Close();
        }
        
        public void SendDataToServer(string message)
        {
            Debug.Log($"Sending data to server: {message}");
            var data = Encode(message);
            NetworkStream.Write(data, 0, data.Length);
        }

        private void OnConnectToServer(IAsyncResult result)
        {
            Debug.Log("Client connected to server. Finishing handshake.");
            _client.EndConnect(result);
            NetworkStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnRead, null);
        }
        
        private void OnRead(IAsyncResult asyncResult)
        {
            Debug.Log("Message received in client");
            var bytesRead = NetworkStream.EndRead(asyncResult);

            if (bytesRead <= 0)
            {
                Debug.Log($"Message received but no bytes read");
                TcpServerManager.Instance.DisconnectClient(this);
                return;
            }

            lock (ReadHandler)
            {
                Debug.Log("Processing message received in client");
                var dataToBroadcast = new byte[bytesRead];
                Array.Copy(ReadBuffer, dataToBroadcast, bytesRead);
                Debug.Log($"Message enqueued in client: { Decode(dataToBroadcast) }");
                _dataReceived.Enqueue(dataToBroadcast);
            }
            
            Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
            NetworkStream?.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnRead, null);
            Debug.Log("Buffer cleared and client listening again");
        }

        public void FlushQueuedMessages()
        {
            while (_dataReceived.Count > 0)
            {
                Debug.Log("Processing client UI pending messages");
                var data = _dataReceived.Dequeue();
                try
                {
                    OnClientMessageReceived.Invoke(data);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}

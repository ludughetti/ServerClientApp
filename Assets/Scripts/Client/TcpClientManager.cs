using System;
using System.Net;
using System.Net.Sockets;
using Messages;
using Server;
using UnityEngine;

namespace Client
{
    public class TcpClientManager : ClientManager
    {
        private TcpClient _client;
        
        public byte[] ReadBuffer = new byte[5000];
        public object ReadHandler = new ();
        public NetworkStream NetworkStream => _client?.GetStream();
        
        public TcpClientManager(TcpClient client)
        {
            _client = client;
        }
        
        public TcpClientManager(TcpClient client, string userName)
        {
            _client = client;
            Username = userName;
        }

        public override void StartClient(IPAddress serverIPAddress, int port)
        {
            Debug.Log($"Starting client... connecting to {serverIPAddress}:{port}");
            _client.BeginConnect(serverIPAddress, port, OnConnectToServer, null);
        }
        
        public override void CloseClient()
        {
            NetworkStream?.Close();
            _client?.Close();
        }
        
        public override void SendDataToServer(int linkedMessageId, string message)
        {
            // We initialize _id = 0 here because the server will assign the message id
            // This is to ensure consistency among all clients
            var newMessage = new ChatMessage(0, linkedMessageId, Username, message);
            Debug.Log($"Sending data to server: '{ Username } - { message }'");
            
            var data = newMessage.EncodeMessage();
            NetworkStream.Write(data, 0, data.Length);
        }

        private void OnConnectToServer(IAsyncResult result)
        {
            Debug.Log("Client connected to server. Finishing handshake.");
            _client.EndConnect(result);
            NetworkStream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnRead, null);
        }
        
        public override void OnRead(IAsyncResult asyncResult)
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
                var data = new byte[bytesRead];
                Array.Copy(ReadBuffer, data, bytesRead);
                
                StoreNewMessage(data);
            }
            
            Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
            NetworkStream?.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnRead, null);
            Debug.Log("Buffer cleared and TCP client listening again");
        }
    }
}

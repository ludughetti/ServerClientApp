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
            _username = userName;
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
        
        public override void SendDataToServer(string message)
        {
            ChatMessage newMessage = new ChatMessage(_username, message);
            Debug.Log($"Sending data to server: '{ _username } - { message }'");
            
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
                
                ChatMessage newMessage = ChatMessage.DecodeMessage(data);
                
                Debug.Log($"Message enqueued in client: '{ newMessage.GetUsername() } - { newMessage.GetMessage() }'");
                _dataReceived.Enqueue(newMessage);
            }
            
            Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
            NetworkStream?.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnRead, null);
            Debug.Log("Buffer cleared and client listening again");
        }
    }
}

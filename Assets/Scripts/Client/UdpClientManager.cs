using System;
using System.Net;
using System.Net.Sockets;
using Messages;
using UnityEngine;

namespace Client
{
    public class UdpClientManager : ClientManager
    {
        private IPEndPoint _serverEndpoint;
        private UdpClient _client;

        public UdpClientManager(string username)
        {
            _username = username;
        }
        
        // Since this constructor is used by the server and the username is saved in each message,
        // we avoid initializing it here
        public UdpClientManager(IPEndPoint clientServerEndpoint)
        {
            _serverEndpoint = clientServerEndpoint;
        }

        public IPEndPoint GetClientEndPoint()
        {
            return _serverEndpoint;
        }

        public override void StartClient(IPAddress serverIPAddress, int port)
        {
            Debug.Log($"Starting UDP client... connecting to {serverIPAddress}:{port}");
            _client = new UdpClient();
            _serverEndpoint = new IPEndPoint(serverIPAddress, port);
            _client.BeginReceive(OnRead, null);
            SendOnConnect();
        }

        public override void CloseClient()
        {
            _client.Close();
        }
        
        public override void SendDataToServer(string message)
        {
            ChatMessage newMessage = new ChatMessage(_username, message);
            Debug.Log($"Sending data to UDP server: { newMessage }");
            
            var data = newMessage.EncodeMessage();
            _client.Send(data, data.Length, _serverEndpoint);
        }
        
        public override void OnRead(IAsyncResult asyncResult)
        {
            Debug.Log("Message received in UDP client. Processing...");
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            var bytesRead = _client.EndReceive(asyncResult, ref endpoint);

            var newMessage = ChatMessage.DecodeMessage(bytesRead);
            
            Debug.Log($"Enqueueing message in UDP client: { newMessage }");
            _dataReceived.Enqueue(newMessage);
            
            _client.BeginReceive(OnRead, _client);
            Debug.Log("Message processed and UDP client listening again");
        }

        private void SendOnConnect()
        {
            SendDataToServer(_onConnectMessage);
        }
    }
}

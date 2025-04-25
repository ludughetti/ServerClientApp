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
            Username = username;
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
        
        public override void SendDataToServer(int linkedMessageId, string message)
        {
            // We initialize _id = 0 here because the server will assign the message id
            // This is to ensure consistency among all clients
            var newMessage = new ChatMessage(0, linkedMessageId, Username, message);
            Debug.Log($"Sending data to UDP server: { newMessage }");
            
            var data = newMessage.EncodeMessage();
            _client.Send(data, data.Length, _serverEndpoint);
        }
        
        public override void OnRead(IAsyncResult asyncResult)
        {
            Debug.Log("Message received in UDP client. Processing...");
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            var data = _client.EndReceive(asyncResult, ref endpoint);

            if (data.Length == 0)
            {
                Debug.Log($"Message received but no bytes read");
                return;
            }
            
            StoreNewMessage(data);
            
            _client.BeginReceive(OnRead, _client);
            Debug.Log("UDP client listening again");
        }

        // In the case of UDP clients we send a default message on connect 
        // so that the server can have the reference for broadcasting other clients' messages
        private void SendOnConnect()
        {
            SendDataToServer(0, OnConnectMessage);
        }
    }
}

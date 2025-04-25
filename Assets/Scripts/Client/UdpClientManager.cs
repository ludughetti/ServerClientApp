using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using static Utils.Encoder;

namespace Client
{
    public class UdpClientManager : ClientManager
    {
        private IPEndPoint _serverEndpoint;
        private UdpClient _client;
        
        public UdpClientManager() { }
        
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
            Debug.Log($"Sending data to UDP server: {message}");
            var data = Encode(message);
            _client.Send(data, data.Length, _serverEndpoint);
        }
        
        public override void OnRead(IAsyncResult asyncResult)
        {
            Debug.Log("Message received in UDP client. Processing...");
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            var bytesRead = _client.EndReceive(asyncResult, ref endpoint);

            Debug.Log($"Enqueueing message in UDP client: { Decode(bytesRead) }");
            _dataReceived.Enqueue(bytesRead);
            
            Debug.Log("Message processed and UDP client listening again");
            _client.BeginReceive(OnRead, _client);
        }

        private void SendOnConnect()
        {
            SendDataToServer(_onConnectMessage);
        }
    }
}

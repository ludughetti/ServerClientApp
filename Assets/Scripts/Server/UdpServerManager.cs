using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Client;
using Messages;
using UnityEngine;
using Utils;

namespace Server
{
    public class UdpServerManager : ServerManager
    {
        public static UdpServerManager Instance => MonoBehaviourSingleton<UdpServerManager>.Instance;
        
        private List<UdpClientManager> _connectedClients = new ();
        private UdpClient _udpServer;
        private IPEndPoint _udpServerEndpoint = new (IPAddress.Any, 0);
        
        public override void StartServer(int portNumber, bool queueUIPendingMessages)
        {
            Debug.Log($"Starting UDP Server on port { portNumber }");
            _udpServer = new UdpClient(portNumber);
            _udpServer.BeginReceive(OnRead, null);
            
            // Define whether to queue incoming messages to display in the UI (only applicable in server-only mode)
            QueueUIPendingMessages = queueUIPendingMessages;
        }

        public override void StopServer()
        {
            Debug.Log("Stopping Server");

            lock (_connectedClients)
                _connectedClients.Clear();
            
            _udpServer?.Close();
        }

        public override void ReceiveAndBroadcastData(byte[] data)
        {
            var newMessage = StoreNewMessage(data);
            Debug.Log($"Broadcasting data to clients: '{ newMessage }'");
            
            foreach (var udpClient in _connectedClients)
            {
                Debug.Log($"Sending message to client ");
                var updatedMessageData = newMessage.EncodeMessage();
                _udpServer.Send(updatedMessageData, updatedMessageData.Length, udpClient.GetClientEndPoint());
            }
        }

        public override void OnRead(IAsyncResult result)
        {
            Debug.Log("Processing new message received in UDP server");
            var dataToBroadcast = _udpServer.EndReceive(result, ref _udpServerEndpoint);

            // If the server doesn't have this client in the list then add it
            if (!_connectedClients.Exists(client => client.GetClientEndPoint().Equals(_udpServerEndpoint)))
            {
                Debug.Log("Adding new client to server list");
                _connectedClients.Add(new UdpClientManager(_udpServerEndpoint));
            }

            ReceiveAndBroadcastData(dataToBroadcast);
            
            _udpServer.BeginReceive(OnRead, null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Client;
using UnityEngine;
using Utils;
using static Utils.Encoder;

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
            _queueUIPendingMessages = queueUIPendingMessages;
        }

        public override void StopServer()
        {
            Debug.Log("Stopping Server");

            lock (_connectedClients)
            {
                foreach (var udpClient in _connectedClients)
                    udpClient.CloseClient();

                _connectedClients.Clear();
            }
            
            _udpServer?.Close();
        }

        public override void ReceiveAndBroadcastData(byte[] data)
        {
            var dataMessage = Decode(data);
            
            // Queue messages in history
            _chatHistory.Enqueue(dataMessage);
            
            // Queue messages for UI
            if (_queueUIPendingMessages)
                _uiPendingMessages.Enqueue(dataMessage);
            
            foreach (var udpClient in _connectedClients)
            {
                Debug.Log($"Broadcasting data to client: { dataMessage }");
                _udpServer.Send(data, data.Length, udpClient.GetClientEndPoint());
            }
        }

        public override void OnRead(IAsyncResult result)
        {
            Debug.Log("Processing new message received in UDP server");
            var dataToBroadcast = _udpServer.EndReceive(result, ref _udpServerEndpoint);

            // If the server doesn't have this client in the list then 
            if (!_connectedClients.Exists(client => client.GetClientEndPoint().Equals(_udpServerEndpoint)))
            {
                Debug.Log("Adding new client to server list");
                _connectedClients.Add(new UdpClientManager(_udpServerEndpoint));
            }
            
            Debug.Log($"Total connected clients: {_connectedClients.Count}");
            
            var dataToQueue = Decode(dataToBroadcast);
            
            // Queue messages in history
            _chatHistory.Enqueue(dataToQueue);
            
            // Queue in pending messages if it's running as server only so that UI is updated
            if (_queueUIPendingMessages)
                _uiPendingMessages.Enqueue(dataToQueue);
            
            Debug.Log($"Broadcasting data to clients. Message: { dataToQueue }");
            ReceiveAndBroadcastData(dataToBroadcast);
            
            _udpServer.BeginReceive(OnRead, null);
        }
    }
}

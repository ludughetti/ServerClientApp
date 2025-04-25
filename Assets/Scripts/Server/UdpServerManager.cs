using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Client;
using UnityEngine;
using static Utils.Encoder;

namespace Server
{
    public class UdpServerManager : ServerManager
    {
        private List<UdpClientManager> _connectedClients = new ();
        private UdpClient _udpServer;
        
        public override void StartServer(int portNumber, bool queueUIPendingMessages)
        {
            Debug.Log($"Starting Server on port { portNumber }");
            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Loopback, portNumber);
            _udpServer = new UdpClient(serverEndpoint);
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
                _udpServer.Send(data, data.Length, udpClient.GetClientEndPoint());
            }
        }

        public override void OnRead(IAsyncResult result)
        {
            Debug.Log("UDP Server received new message");
            var remoteClientIP = new IPEndPoint(IPAddress.Any, 0);
            var dataToBroadcast = _udpServer.EndReceive(result, ref remoteClientIP);

            // If the server doesn't have this client in the list then 
            if (!_connectedClients.Exists(client => client.GetClientEndPoint().Equals(remoteClientIP)))
            {
                _connectedClients.Add(new UdpClientManager(remoteClientIP));
            }
            
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

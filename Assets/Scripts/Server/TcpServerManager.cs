using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Client;
using Messages;
using UnityEngine;
using Utils;
using static Utils.Encoder;

namespace Server
{
    public class TcpServerManager : ServerManager
    {
        public static TcpServerManager Instance => MonoBehaviourSingleton<TcpServerManager>.Instance;
        private List<TcpClientManager> _connectedClients = new ();
        private TcpListener _listener;
        
        public override void StartServer(int portNumber, bool queueUIPendingMessages)
        {
            Debug.Log($"Starting TCP Server on port { portNumber }");
            _listener = new TcpListener(IPAddress.Any, portNumber);

            _listener.Start();
            _listener.BeginAcceptTcpClient(OnClientConnected, null);
            
            // Define whether to queue incoming messages to display in the UI (only applicable in server-only mode)
            QueueUIPendingMessages = queueUIPendingMessages;
        }

        public override void StopServer()
        {
            Debug.Log("Stopping Server");
            _listener?.Stop();

            lock (_connectedClients)
            {
                foreach (var tcpClient in _connectedClients)
                    tcpClient.CloseClient();

                _connectedClients.Clear();
            }
        }

        public override void ReceiveAndBroadcastData(byte[] data)
        {
            var newMessage = StoreNewMessage(data);
            Debug.Log($"Broadcasting data to clients: { newMessage }");
            
            foreach (var tcpClient in _connectedClients)
            {
                try
                {
                    Debug.Log($"Sending message to client ");
                    var updatedMessageData = newMessage.EncodeMessage();
                    tcpClient.NetworkStream.Write(updatedMessageData, 0, updatedMessageData.Length);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void DisconnectClient(TcpClientManager client)
        {
            client.CloseClient();
            _connectedClients.Remove(client);

            Debug.Log("Client disconnected");
        }

        private void OnClientConnected(IAsyncResult result)
        {
            Debug.Log("Client connected");
            var client = _listener.EndAcceptTcpClient(result);

            // Start waiting for next client
            _listener.BeginAcceptTcpClient(OnClientConnected, null);

            // Handle connected client asynchronously
            StartClientRead(client);
        }

        private void StartClientRead(TcpClient client)
        {
            Debug.Log("Listening to messages for new client");
            var clientManager = new TcpClientManager(client);

            // Add new client to the list of connected clients
            _connectedClients.Add(clientManager);
            
            try
            {
                Debug.Log("New client connected to server");
                clientManager.NetworkStream.BeginRead(clientManager.ReadBuffer, 0, 
                    clientManager.ReadBuffer.Length, OnRead, clientManager);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override void OnRead(IAsyncResult result)
        {
            Debug.Log("Server received new message");
            var clientManager = (TcpClientManager) result.AsyncState;
            
            var bytesRead = clientManager.NetworkStream.EndRead(result);

            if (bytesRead <= 0)
            {
                Debug.Log("Message received but server read no bytes");
                DisconnectClient(clientManager);
                return;
            }

            byte[] dataToBroadcast;

            lock (clientManager.ReadHandler)
            {
                Debug.Log("Processing message received in server");
                dataToBroadcast = new byte[bytesRead];
                Array.Copy(clientManager.ReadBuffer, dataToBroadcast, bytesRead);
            }
            
            ReceiveAndBroadcastData(dataToBroadcast);
            
            Array.Clear(clientManager.ReadBuffer, 0, clientManager.ReadBuffer.Length);
            clientManager.NetworkStream.BeginRead(clientManager.ReadBuffer, 0, 
                clientManager.ReadBuffer.Length, OnRead, clientManager);
        }
    }
}

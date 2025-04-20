using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Client;
using Unity.VisualScripting;
using Utils;
using UnityEngine;
using static Utils.Encoder;

namespace Server
{
    public class TcpServerManager : MonoBehaviourSingleton<TcpServerManager>
    {
        private List<TcpClientManager> _connectedClients = new ();
        private TcpListener _listener;
        private Queue<string> _uiPendingMessages = new ();
        private Queue<string> _chatHistory = new ();
        private bool _queueUIPendingMessages;
        
        public Action<byte[]> OnServerMessageReceived;
        
        public void StartServer(int portNumber, bool queueUIPendingMessages)
        {
            Debug.Log($"Starting Server on port { portNumber }");
            _listener = new TcpListener(IPAddress.Any, portNumber);

            _listener.Start();
            _listener.BeginAcceptTcpClient(OnClientConnected, null);
            
            // Define whether to queue incoming messages to display in the UI (only applicable in server-only mode)
            _queueUIPendingMessages = queueUIPendingMessages;
        }

        public void StopServer()
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

        private void ReceiveAndBroadcastData(byte[] data)
        {
            var dataMessage = Decode(data);
            
            // Queue messages in history
            _chatHistory.Enqueue(dataMessage);
            
            // 
            if (_queueUIPendingMessages)
                _uiPendingMessages.Enqueue(dataMessage);
            
            foreach (var tcpClient in _connectedClients)
            {
                try
                {
                    Debug.Log($"Broadcasting data to client: { dataMessage }");
                    tcpClient.NetworkStream.Write(data, 0, data.Length);
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

        private void OnRead(IAsyncResult result)
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
            
            Debug.Log($"Broadcasting data to clients. Message: {Decode(dataToBroadcast)}");
            ReceiveAndBroadcastData(dataToBroadcast);
            
            Array.Clear(clientManager.ReadBuffer, 0, clientManager.ReadBuffer.Length);
            clientManager.NetworkStream.BeginRead(clientManager.ReadBuffer, 0, 
                clientManager.ReadBuffer.Length, OnRead, clientManager);
        }

        public void FlushEnqueuedMessages()
        {
            while (_uiPendingMessages.Count > 0)
            {
                Debug.Log("Processing server UI pending messages");
                var message = _uiPendingMessages.Dequeue();
                
                OnServerMessageReceived?.Invoke(Encode(message));
            }
        }
    }
}

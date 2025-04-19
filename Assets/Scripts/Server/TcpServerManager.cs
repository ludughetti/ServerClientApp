using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Client;
using Common;
using UnityEngine;

namespace Server
{
    public class TcpServerManager : MonoBehaviourSingleton<TcpServerManager>
    {
        private List<TcpClientManager> _connectedClients = new ();
        private TcpListener _listener;

        public void StartServer(int portNumber)
        {
            Debug.Log("Start Server");
            _listener = new TcpListener(IPAddress.Any, portNumber);

            _listener.Start();
            _listener.BeginAcceptTcpClient(OnClientConnected, null);
        }

        public void StopServer()
        {
            Debug.Log("Stop Server");
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
            foreach (var tcpClient in _connectedClients)
            {
                try
                {
                    Debug.Log($"Broadcasting data to client. Is NetworkStream available? {tcpClient.NetworkStream != null}");
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

            _connectedClients.Add(clientManager);
            try
            {
                Debug.Log($"Client connected. Is NetworkStream available? {clientManager.NetworkStream != null}");
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
            Debug.Log("Message received");
            TcpClientManager clientManager = (TcpClientManager) result.AsyncState;
            
            var bytesRead = clientManager.NetworkStream.EndRead(result);

            if (bytesRead <= 0)
            {
                DisconnectClient(clientManager);
                return;
            }

            byte[] dataToBroadcast;

            lock(clientManager.ReadHandler)
                dataToBroadcast = clientManager.ReadBuffer.TakeWhile(b => (char) b != '\0').ToArray();
            
            Debug.Log($"Broadcasting data to clients. Message: {Encoding.UTF8.GetString(dataToBroadcast)}");
            ReceiveAndBroadcastData(dataToBroadcast);
            
            Array.Clear(clientManager.ReadBuffer, 0, clientManager.ReadBuffer.Length);
            clientManager.NetworkStream.BeginRead(clientManager.ReadBuffer, 0, 
                clientManager.ReadBuffer.Length, OnRead, null);
        }
    }
}

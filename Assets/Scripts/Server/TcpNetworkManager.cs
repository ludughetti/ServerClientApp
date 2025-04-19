using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Client;
using Common;
using UnityEngine;

namespace Server
{
    public class TcpNetworkManager : MonoBehaviourSingleton<TcpNetworkManager>
    {
        public bool IsServer { get; private set; }

        private List<TcpConnectedClient> _serverClients = new List<TcpConnectedClient>();
        private TcpConnectedClient _selfClient;
        private TcpListener _listener;
        private bool _clientHasJustConnected;

        public Action OnClientConnected;
        public Action<byte[]> OnDataReceived;
        public Action<byte[]> OnDataSent;

        private void Update()
        {
            /*if (IsServer)
                UpdateServer();
            else
                UpdateClient();*/
        }

        private void OnDestroy()
        {
            _listener?.Stop();

            if (IsServer)
            {
                foreach (var tcpClient in _serverClients)
                                tcpClient.CloseClient();
            }
            else
            {
                _selfClient?.CloseClient();
            }
        }

        public void StartServer(int port)
        {
            Debug.Log("StartServer");
            IsServer = true;
            _listener = new TcpListener(IPAddress.Any, port);

            _listener.Start();
            _listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
        }

        public void StartClient(IPAddress serverIPAddress, int port)
        {
            Debug.Log("StartClient");
            IsServer = false;
            var client = new TcpClient();

            _selfClient = new TcpConnectedClient(client);
            client.BeginConnect(serverIPAddress, port, OnClientConnect, null);
        }

        public void ReceiveData(byte[] data)
        {
            Debug.Log("ReceiveData");
            OnDataReceived?.Invoke(data);
        }

        public void DisconnectClient(TcpConnectedClient client)
        {
            if (!IsServer)
                return;
            
            if(_serverClients.Contains(client))
                _serverClients.Remove(client);
        }

        public void BroadcastData(byte[] data)
        {
            Debug.Log("BroadcastData");
            foreach (var client in _serverClients)
                client.SendData(data);
            
            OnDataSent?.Invoke(data);
        }

        public void SendDataToServer(byte[] data)
        {
            Debug.Log("SendDataToServer");
            _selfClient?.SendData(data);
            OnDataSent?.Invoke(data);
        }

        private void UpdateServer()
        {
            foreach (var tcpClient in _serverClients)
                tcpClient.FlushReceivedData();
        }

        private void UpdateClient()
        {
            if (_clientHasJustConnected)
            {
                _clientHasJustConnected = false;
                OnClientConnected?.Invoke();
            }
            
            _selfClient?.FlushReceivedData();
        }

        private void OnClientConnectToServer(IAsyncResult asyncResult)
        {
            Debug.Log("OnClientConnectToServer");
            var client = _listener.EndAcceptTcpClient(asyncResult);
            var tcpConnectedClient = new TcpConnectedClient(client);
            
            _serverClients.Add(tcpConnectedClient);
            _listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
        }

        private void OnClientConnect(IAsyncResult asyncResult)
        {
            Debug.Log("OnClientConnect");
            _selfClient.OnEndConnection(asyncResult);
            _clientHasJustConnected = true;
        }
    }
}

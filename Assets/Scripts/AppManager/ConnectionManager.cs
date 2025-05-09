using System.Net;
using System.Net.Sockets;
using Client;
using Server;
using UnityEngine;

namespace AppManager
{
    public class ConnectionManager : MonoBehaviour
    {
        private ServerManager _serverManager;
        private ClientManager _clientManager;
        private IPAddress _serverIp;
        private bool _isServerOnlyApp;
        private bool _isClientOnlyApp;
        private bool _isConnectionManagerActive;

        private void Update()
        {
            if (_isConnectionManagerActive)
                FlushData();
        }

        // Wrapper for headless mode
        public void StartConnection(int port, string networkType)
        {
            if ("TCP".Equals(networkType) || "UDP".Equals(networkType))
                StartConnection(true, false, string.Empty, port, networkType, "");
            else
                Debug.LogError($"Unknown network type: {networkType}. Server will not start.");
        }
        
        public void StartConnection(bool isServerOnlyApp, bool isClientOnlyApp, string ipAddress, int port, 
            string networkType, string userName)
        {
            Debug.Log("Connecting...\n" +
                      $"Is Client only? { isClientOnlyApp }, " +
                      $"Is Server only? { isServerOnlyApp }, " +
                      $"IPAddress: { ipAddress }, " +
                      $"PortNumber: { port }, " +
                      $"NetworkType: { networkType }");
            
            _isServerOnlyApp = isServerOnlyApp;
            _isClientOnlyApp = isClientOnlyApp;
            
            // Is server only or is server-client
            if (!isClientOnlyApp)
            {
                HandleServerStart(port, networkType, isServerOnlyApp);

                if (isServerOnlyApp)
                    return;
            }
            
            // If it's a client only we use the address entered in the UI, otherwise we loopback
            _serverIp = isClientOnlyApp ? IPAddress.Parse(ipAddress) : IPAddress.Loopback;

            // Initialize client
            HandleClientStart(_serverIp, port, networkType, userName);
        }

        public void EndConnection()
        {
            Debug.Log($"Disconnecting...\n" +
                      $"Is Client only? { _isClientOnlyApp }, " +
                      $"Is Server only? { _isServerOnlyApp }, " +
                      $"IPAddress: { _serverIp } ");
            
            _isConnectionManagerActive = false;
            
            if (!_isServerOnlyApp)
                _clientManager.CloseClient();
            
            // If it's client only we don't need to close the server
            if (_isClientOnlyApp) 
                return;
            
            _serverManager.StopServer();
        }

        public ClientManager GetClientManager()
        {
            return _clientManager;
        }
        
        public ServerManager GetServerManager()
        {
            return _serverManager;
        }

        public void HandleClientMessageSent(int linkedMessageId, string message)
        {
            _clientManager.SendDataToServer(linkedMessageId, message);
        }

        private void HandleServerStart(int port, string networkType, bool isServerOnlyApp)
        {
            if ("TCP".Equals(networkType))
            {
                TcpServerManager.Instance.StartServer(port, isServerOnlyApp);
                _serverManager = TcpServerManager.Instance;
            }
            else
            {
                UdpServerManager.Instance.StartServer(port, isServerOnlyApp);
                _serverManager = UdpServerManager.Instance;
            }
            
            _isConnectionManagerActive = true;
        }

        private void HandleClientStart(IPAddress ipAddress, int port, string networkType, string userName)
        {
            _clientManager = "TCP".Equals(networkType) ? 
                new TcpClientManager(new TcpClient(), userName) :
                new UdpClientManager(userName);
            
            _clientManager.StartClient(ipAddress, port);
            
            _isConnectionManagerActive = true;
        }

        private void FlushData()
        {
            if (_clientManager != null && !_isServerOnlyApp)
                _clientManager.FlushQueuedMessages();
            else if (!_isClientOnlyApp)
                _serverManager.FlushEnqueuedMessages();
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using Client;
using Navigation;
using Server;
using UI;
using UnityEngine;
using static Utils.Validator;

namespace AppManager
{
    public class ApplicationManager : MonoBehaviour
    {
        [SerializeField] private NavigationManager navigationManager;
        [SerializeField] private ConnectionManager connectionManager;
        [SerializeField] private UIServerClientHandler uiServerClientHandler;
        [SerializeField] private UIChatHandler uiChatHandler;
        [SerializeField] private MenuDataSource chatroomMenu;

        private bool _isClientOnlyApp;
        private bool _isServerOnlyApp;
        private IPAddress _serverIP;
        private int _serverPort;
        private TcpClientManager _client;
        
        private void Awake()
        {
            ValidateDependencies();
        }

        private void OnEnable()
        {
            navigationManager.OnMenuChange += CheckMenuChange;
            uiChatHandler.OnUserMessageSent += SendUserMessage;
        }

        private void OnDisable()
        {
            navigationManager.OnMenuChange -= CheckMenuChange;
            uiChatHandler.OnUserMessageSent -= SendUserMessage;
        }
        
        private void Update()
        {
            if(_client != null && (_isClientOnlyApp || !_isServerOnlyApp))
                _client.FlushQueuedMessages();
            else if (_isServerOnlyApp)
                TcpServerManager.Instance.FlushEnqueuedMessages();
        }

        private void CheckMenuChange(string newMenuId)
        {
            if (chatroomMenu.GetMenuId() == newMenuId)
                Connect();
            else
                Disconnect();
        }

        private void Connect()
        {
            Debug.Log($"Connecting...\n" +
                      $"Is Client? {uiServerClientHandler.IsClientOnlyApp()}, " +
                      $"IPAddress: {uiServerClientHandler.GetServerIP()}, " +
                      $"PortNumber: {uiServerClientHandler.GetPort()}");

            _isClientOnlyApp = uiServerClientHandler.IsClientOnlyApp();
            _isServerOnlyApp = uiServerClientHandler.IsServerOnlyApp();
            var port = uiServerClientHandler.GetPort();
            _serverPort = Convert.ToInt32(port);

            // Is server only or is server-client
            if (_isServerOnlyApp || !_isClientOnlyApp)
            {
                TcpServerManager.Instance.StartServer(_serverPort, _isServerOnlyApp);
                
                // If it's server only, early exit
                if (_isServerOnlyApp)
                {
                    // Since this app will only be running the server, we subscribe the event so that UI gets updated too
                    TcpServerManager.Instance.OnServerMessageReceived += uiChatHandler.OnDataReceived;
                    return;
                }
            }
            
            // If it's a client only we use the address entered in the UI, otherwise we loopback
            _serverIP = _isClientOnlyApp ? IPAddress.Parse(uiServerClientHandler.GetServerIP()) : IPAddress.Loopback;
            
            _client = new TcpClientManager(new TcpClient());
            
            //Subscribe to client receive event so that UI is updated
            _client.OnClientMessageReceived += uiChatHandler.OnDataReceived;
            _client.StartClient(_serverIP, _serverPort);
        }

        private void SendUserMessage(string message)
        {
            _client.SendDataToServer(message);
        }

        private void Disconnect()
        {
            Debug.Log($"Disconnecting...\n" +
                      $"Is Client? {uiServerClientHandler.IsClientOnlyApp()}, " +
                      $"IPAddress: {uiServerClientHandler.GetServerIP()}, " +
                      $"PortNumber: {uiServerClientHandler.GetPort()}");

            if (_isClientOnlyApp || !_isServerOnlyApp)
            {
                // Unsubscribe upon client disconnect 
                _client.OnClientMessageReceived -= uiChatHandler.OnDataReceived;
                _client.CloseClient();
                
                // If it's client only we don't need to close the server
                if (_isClientOnlyApp) 
                    return;
            }
            
            TcpServerManager.Instance.StopServer();

            // If we were only running the server, we unsubscribe from the UI update event 
            if (_isServerOnlyApp)
                TcpServerManager.Instance.OnServerMessageReceived -= uiChatHandler.OnDataReceived;
        }

        private void ValidateDependencies()
        {
            enabled = IsDependencyConfigured(name, "Navigation Manager", navigationManager) && 
                      IsDependencyConfigured(name, "ConnectionManager", connectionManager) && 
                      IsDependencyConfigured(name, "UI Server Client Handler", uiServerClientHandler) && 
                      IsDependencyConfigured(name, "UI Chat Handler", uiChatHandler) && 
                      IsDependencyConfigured(name, "Chatroom Menu", chatroomMenu);
        }
    }
}

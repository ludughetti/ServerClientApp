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

        private bool _isClientApp;
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
            if(_isClientApp && _client != null)
                _client.FlushQueuedMessages();
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
                      $"Is Client? {uiServerClientHandler.IsClientApp()}, " +
                      $"IPAddress: {uiServerClientHandler.GetServerIP()}, " +
                      $"PortNumber: {uiServerClientHandler.GetPort()}");

            _isClientApp = uiServerClientHandler.IsClientApp();
            var port = uiServerClientHandler.GetPort();
            _serverPort = Convert.ToInt32(port);

            if (_isClientApp)
            {
                _serverIP = IPAddress.Parse(uiServerClientHandler.GetServerIP());
                _client = new TcpClientManager(new TcpClient());
                
                //Subscribe to client receive event so that UI is updated
                _client.OnMessageReceived += uiChatHandler.OnDataReceived;
                _client.StartClient(_serverIP, _serverPort);
            }
            else
            {
                TcpServerManager.Instance.StartServer(_serverPort);
            }
        }

        private void SendUserMessage(string message)
        {
            _client.SendDataToServer(message);
        }

        private void Disconnect()
        {
            Debug.Log($"Disconnecting...\n" +
                      $"Is Client? {uiServerClientHandler.IsClientApp()}, " +
                      $"IPAddress: {uiServerClientHandler.GetServerIP()}, " +
                      $"PortNumber: {uiServerClientHandler.GetPort()}");

            if (_isClientApp)
            {
                // Unsubscribe upon client disconnect 
                _client.OnMessageReceived -= uiChatHandler.OnDataReceived;
                _client.CloseClient();
            }
            else
                TcpServerManager.Instance.StopServer();
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

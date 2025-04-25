using System;
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
        
        private void Awake()
        {
            ValidateDependencies();
        }

        private void OnEnable()
        {
            navigationManager.OnMenuChange += CheckMenuChange;
            uiChatHandler.OnUserMessageSent += connectionManager.HandleClientMessageSent;
        }

        private void OnDisable()
        {
            navigationManager.OnMenuChange -= CheckMenuChange;
            uiChatHandler.OnUserMessageSent -= connectionManager.HandleClientMessageSent;
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
            _isClientOnlyApp = uiServerClientHandler.IsClientOnlyApp();
            _isServerOnlyApp = uiServerClientHandler.IsServerOnlyApp();
            var port = uiServerClientHandler.GetPort();

            // ConnectionManager will decide how to start up the server/client and whether to use TCP/UDP
            connectionManager.StartConnection(_isServerOnlyApp, _isClientOnlyApp, 
                uiServerClientHandler.GetServerIP(), Convert.ToInt32(port), "TCP");
            
            // If it's server only we subscribe the event so that UI gets updated too and early exit
            if (_isServerOnlyApp)
            {
                connectionManager.GetServerManager().OnDataReceived += uiChatHandler.OnDataReceived;
                return;
            }
            
            // If it's a client subscribe to client receive event so that UI is updated
            connectionManager.GetClientManager().OnDataReceived += uiChatHandler.OnDataReceived;
        }

        private void Disconnect()
        {
            Debug.Log($"Disconnecting...\n" +
                      $"Is Client? {uiServerClientHandler.IsClientOnlyApp()}, " +
                      $"IPAddress: {uiServerClientHandler.GetServerIP()}, " +
                      $"PortNumber: {uiServerClientHandler.GetPort()}");

            connectionManager.EndConnection();
            
            // Unsubscribe upon client disconnect 
            if (!_isServerOnlyApp)
                connectionManager.GetClientManager().OnDataReceived -= uiChatHandler.OnDataReceived;
            
            TcpServerManager.Instance.StopServer();

            // If we were only running the server, we unsubscribe from the UI update event 
            if (_isServerOnlyApp)
                TcpServerManager.Instance.OnDataReceived -= uiChatHandler.OnDataReceived;
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

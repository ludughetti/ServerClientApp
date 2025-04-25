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
        }

        private void OnDisable()
        {
            navigationManager.OnMenuChange -= CheckMenuChange;
            uiChatHandler.OnUserMessageSent -= connectionManager.HandleClientMessageSent;
        }

        public void ExitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        public bool IsServerOnlyApp()
        {
            return _isClientOnlyApp;
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
            
            Debug.Log($"Network type is {uiServerClientHandler.GetNetworkType()}");

            // ConnectionManager will decide how to set up and start up the server/client
            connectionManager.StartConnection(_isServerOnlyApp, _isClientOnlyApp, uiServerClientHandler.GetServerIP(), 
                Convert.ToInt32(port), uiServerClientHandler.GetNetworkType(), uiServerClientHandler.GetUsername());
            
            // If it's server only we subscribe the event so that UI gets updated too and early exit
            if (_isServerOnlyApp)
            {
                connectionManager.GetServerManager().OnDataReceived += uiChatHandler.OnDataReceived;
                return;
            }
            
            // If it's a client subscribe to events so that UI is updated
            uiChatHandler.OnUserMessageSent += connectionManager.HandleClientMessageSent;
            connectionManager.GetClientManager().OnDataReceived += uiChatHandler.OnDataReceived;
        }

        private void Disconnect()
        {
            // ConnectionManager decides what to close/disconnect
            connectionManager.EndConnection();
            
            // Unsubscribe upon client disconnect 
            if (!_isServerOnlyApp)
            {
                uiChatHandler.OnUserMessageSent -= connectionManager.HandleClientMessageSent;
                connectionManager.GetClientManager().OnDataReceived -= uiChatHandler.OnDataReceived;
            }
            
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

using System;
using System.Net;
using System.Text;
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
        
        // TODO: Replace with custom classes
        [SerializeField] private TcpNetworkManager tcpNetworkManager;
        
        private bool _isClientApp;
        private IPAddress _serverIP;
        private int _serverPort;
        
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

        private void CheckMenuChange(string newMenuId)
        {
            if (chatroomMenu.GetMenuId() == newMenuId)
                Connect();
            else
                Disconnect();
        }

        // TODO: Replace with calls to each server/client dedicated class
        private void Connect()
        {
            Debug.Log($"Connecting...\n" +
                      $"Is Client? {uiServerClientHandler.IsClientApp()}, " +
                      $"IPAddress: {uiServerClientHandler.GetServerIP()}, " +
                      $"PortNumber: {uiServerClientHandler.GetPort()}");
            
            _isClientApp = uiServerClientHandler.IsClientApp();
            string port = uiServerClientHandler.GetPort();
            _serverPort = Convert.ToInt32(port);

            if (uiServerClientHandler.IsClientApp())
            {
                _serverIP = IPAddress.Parse(uiServerClientHandler.GetServerIP());
                TcpNetworkManager.Instance.StartClient(_serverIP, _serverPort);
            }
            else
            {
                TcpNetworkManager.Instance.StartServer(_serverPort);
            }
        }

        // TODO: Replace with calls to each server/client dedicated class
        private void SendUserMessage(string message)
        {
            Debug.Log("Message Sent");
            byte[] data = Encoding.UTF8.GetBytes(message);

            if (tcpNetworkManager.IsServer)
            {
                TcpNetworkManager.Instance.BroadcastData(data);
            }
            else
            {
                TcpNetworkManager.Instance.SendDataToServer(data);
            }
        }

        // TODO: Replace with calls to each server/client dedicated class
        private void Disconnect()
        {
            Debug.Log($"Disconnecting...\n" +
                      $"Is Client? {uiServerClientHandler.IsClientApp()}, " +
                      $"IPAddress: {uiServerClientHandler.GetServerIP()}, " +
                      $"PortNumber: {uiServerClientHandler.GetPort()}");
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

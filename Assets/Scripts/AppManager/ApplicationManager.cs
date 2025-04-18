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
        [SerializeField] private MenuDataSource chatroomMenu;
        
        // TODO: Replace with custom classes
        [SerializeField] private TcpNetworkManager tcpNetworkManager;
        
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
        }

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
                      IsDependencyConfigured(name, "Chatroom Menu", chatroomMenu);
        }
    }
}

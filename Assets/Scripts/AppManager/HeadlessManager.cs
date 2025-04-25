using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AppManager
{
    public class HeadlessManager : MonoBehaviour
    {
        [SerializeField] private string mainScene = "MainApp";
        [SerializeField] private GameObject managerObject;
        [SerializeField] private ConnectionManager connectionManager;
        [SerializeField] private string defaultPort = "2000";
        [SerializeField] private string defaultProtocol = "TCP";

        private void Start()
        {
            // If it's running headless, enable the ConnectionManager and let it set up everything
            // Else, load the scene with the AppManager
            if (Application.isBatchMode)
            {
                Debug.Log("Running in batch mode. Initializing server...");
                // Parse command-line arguments
                var args = Environment.GetCommandLineArgs();
                var port = GetArg(args, "-port", defaultPort);
                var protocol = GetArg(args, "-protocol", defaultProtocol);
                
                StartServer(Convert.ToInt32(port), protocol);
            }
            else
            {
                Debug.Log("Running with UI. Loading main scene...");
                SceneManager.LoadScene(mainScene);
            }
        }

        private void StartServer(int port, string networkType)
        {
            managerObject.SetActive(true);
            connectionManager.StartConnection(port, networkType);
        }
        
        private void ShutdownServer()
        {
            connectionManager.EndConnection();
            managerObject.SetActive(false);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
        
        string GetArg(string[] args, string name, string defaultValue)
        {
            int index = Array.IndexOf(args, name);
            if (index >= 0 && index + 1 < args.Length)
                return args[index + 1];
            return defaultValue;
        }
    }
}

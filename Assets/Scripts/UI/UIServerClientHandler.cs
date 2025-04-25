using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils.Validator;

namespace UI
{
    public class UIServerClientHandler : MonoBehaviour
    {
        [SerializeField] private Toggle isClientToggle;
        [SerializeField] private Toggle isServerOnlyToggle;
        [SerializeField] private TMP_InputField serverIPInputField;
        [SerializeField] private TMP_InputField portText;
        [SerializeField] private TMP_InputField usernameText;
        [SerializeField] private TMP_Dropdown networkDropdown;

        private bool _isClientApp;
        private bool _isServerOnlyApp;

        private void Awake()
        {
            ValidateDependencies();
            _isClientApp = isClientToggle.isOn;
            serverIPInputField.interactable = _isClientApp;
        }

        public bool IsClientOnlyApp()
        {
            return _isClientApp;
        }
        
        public bool IsServerOnlyApp()
        {
            return _isServerOnlyApp;
        }

        public string GetServerIP()
        {
            return _isClientApp ? serverIPInputField.text : string.Empty;
        }

        public string GetPort()
        {
            return portText.text;
        }

        public string GetNetworkType()
        {
            return networkDropdown.options[networkDropdown.value].text;
        }

        public string GetUsername()
        {
            return usernameText.text;
        }

        public void OnClientToggleValueChanged(bool isClient)
        {
            _isClientApp = isClient;
            serverIPInputField.interactable = isClient;
            isServerOnlyToggle.interactable = !isClient;
            
            Debug.Log($"Toggle triggered. Final values: isClient = {_isClientApp}, is ServerIP interactable = {serverIPInputField.interactable}");
        }
        
        public void OnServerOnlyValueChanged(bool isServerOnlyApp)
        {
            _isServerOnlyApp = isServerOnlyApp;
            isClientToggle.interactable = !isServerOnlyApp;
            
            Debug.Log($"Button clicked. Final values: isServerClientApp = {_isServerOnlyApp}, is ServerIP interactable = {serverIPInputField.interactable}");
        }
        
        private void ValidateDependencies()
        {
            enabled = IsDependencyConfigured(name, "Is Client Toggle", isClientToggle) && 
                      IsDependencyConfigured(name, "Server IP Field", serverIPInputField) &&
                      IsDependencyConfigured(name, "Port Text", portText);
        }
    }
}

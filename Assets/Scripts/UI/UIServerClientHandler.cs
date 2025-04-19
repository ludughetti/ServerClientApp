using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils.Validator;

namespace UI
{
    public class UIServerClientHandler : MonoBehaviour
    {
        [SerializeField] private Toggle isClientToggle;
        [SerializeField] private TMP_InputField serverIPInputField;
        [SerializeField] private TMP_InputField portText;

        private bool _isClientApp;

        private void Awake()
        {
            ValidateDependencies();
            _isClientApp = isClientToggle.isOn;
            serverIPInputField.interactable = _isClientApp;
        }

        public bool IsClientApp()
        {
            return _isClientApp;
        }

        public string GetServerIP()
        {
            return _isClientApp ? serverIPInputField.text : string.Empty;
        }

        public string GetPort()
        {
            return portText.text;
        }

        public void OnClientToggleValueChanged(bool isClient)
        {
            _isClientApp = isClient;
            serverIPInputField.interactable = isClient;
            
            Debug.Log($"Toggle triggered. Final values: isClient = {_isClientApp}, is ServerIP interactable = {serverIPInputField.interactable}");
        }
        
        private void ValidateDependencies()
        {
            enabled = IsDependencyConfigured(name, "Is Client Toggle", isClientToggle) && 
                      IsDependencyConfigured(name, "Server IP Field", serverIPInputField) &&
                      IsDependencyConfigured(name, "Port Text", portText);
        }
    }
}

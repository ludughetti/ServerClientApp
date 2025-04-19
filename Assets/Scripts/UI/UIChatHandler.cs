using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils.Validator;

namespace UI
{
    public class UIChatHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text chatHistory;
        [SerializeField] private ScrollRect chatHistoryScrollRect;
        [SerializeField] private TMP_InputField userInput;
        [SerializeField] private TMP_Text usersList;
        [SerializeField] private string defaultInputMessage = "Send a message...";

        public Action<string> OnUserMessageSent;
    
        private void Awake()
        {
            ValidateDependencies();
            
            chatHistory.text = string.Empty;
            userInput.text = defaultInputMessage;
        }

        public void OnSendButtonClick()
        {
            Debug.Log("OnSendButtonClick");
            if (string.IsNullOrEmpty(userInput.text))
                return;
            
            OnUserMessageSent?.Invoke(userInput.text);
            
            userInput.text = string.Empty;
        }
        
        public void OnDataReceived(byte[] data)
        {
            Debug.Log("OnMessageReceived invoked");
            UpdateChatHistory(Encoding.UTF8.GetString(data));
            UpdateScroll();
        }
        
        private void UpdateChatHistory(string message)
        {
            Debug.Log($"UpdateChatHistory:  {message}");
            chatHistory.text += message + Environment.NewLine;
        }

        private void UpdateScroll()
        {
            Debug.Log("UpdateScroll");
            chatHistoryScrollRect.verticalNormalizedPosition = 0f;
        }
        
        private void ValidateDependencies()
        {
            enabled = IsDependencyConfigured(name, "Chat History", chatHistory) && 
                      IsDependencyConfigured(name, "User Input", userInput) &&
                      IsDependencyConfigured(name, "User List", usersList);
        }    
    }
}

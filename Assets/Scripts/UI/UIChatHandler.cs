using System;
using Messages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils.Validator;

namespace UI
{
    public class UIChatHandler : MonoBehaviour
    {
        [SerializeField] private ScrollRect chatHistoryScrollRect;
        [SerializeField] private TMP_InputField userInput;
        [SerializeField] private TMP_Text usersList;
        [SerializeField] private Transform chatContent;
        [SerializeField] private GameObject chatMessagePrefab;
        [SerializeField] private string defaultInputMessage = "Send a message...";

        public Action<string> OnUserMessageSent;
    
        private void Awake()
        {
            ValidateDependencies();
            
            userInput.text = defaultInputMessage;
        }

        public void OnSendButtonClick()
        {
            Debug.Log("Send message button clicked");
            if (string.IsNullOrEmpty(userInput.text))
                return;
            
            OnUserMessageSent?.Invoke(userInput.text);
            
            userInput.text = string.Empty;
        }
        
        public void OnDataReceived(ChatMessage chatMessage)
        {
            Debug.Log("UI processing new message");
            UpdateChatHistory(chatMessage);
            UpdateScroll();
        }

        public void ResetChatHistory()
        {
            //chatContent.removeAllChildren();
        }
        
        private void UpdateChatHistory(ChatMessage chatMessage)
        {
            Debug.Log($"Updating Chat history with new message: {chatMessage}");
            //chatHistory.text += message + Environment.NewLine;
            
            // Instantiate new message bubble and populate
            var newMessage = Instantiate(chatMessagePrefab, chatContent);
            
            var texts = newMessage.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = chatMessage.GetUsername(); // or skip if you don’t want names
            texts[1].text = chatMessage.GetMessage();
        }

        private void UpdateScroll()
        {
            Debug.Log("Updating Chat history scroll");
            chatHistoryScrollRect.verticalNormalizedPosition = 0f;
        }
        
        private void ValidateDependencies()
        {
            enabled = IsDependencyConfigured(name, "User Input", userInput) &&
                      IsDependencyConfigured(name, "User List", usersList);
        }    
    }
}

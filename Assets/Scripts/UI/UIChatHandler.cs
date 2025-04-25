using System;
using System.Collections;
using AppManager;
using Messages;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Utils.Validator;

namespace UI
{
    public class UIChatHandler : MonoBehaviour
    {
        [SerializeField] private ApplicationManager applicationManager;
        [SerializeField] private ScrollRect chatHistoryScrollRect;
        [SerializeField] private TMP_InputField userInput;
        [SerializeField] private TMP_Text usersList;
        [SerializeField] private Transform chatContent;
        [SerializeField] private GameObject inputPanel;
        [SerializeField] private GameObject chatMessagePrefab;
        [SerializeField] private GameObject replyingToPanel;
        [SerializeField] private TMP_Text replyingToUserName;
        [SerializeField] private TMP_Text replyingToMessageText;
        [SerializeField] private string defaultInputMessage = "Send a message...";
        
        private int _messageReplyToId;

        public Action<int, string> OnUserMessageSent;
    
        private void Awake()
        {
            ValidateDependencies();
            
            userInput.text = defaultInputMessage;
            // If it's server only, hide input section
            inputPanel.SetActive(!applicationManager.IsServerOnlyApp());
        }

        public void OnSendButtonClick()
        {
            Debug.Log("Send message button clicked");
            if (string.IsNullOrEmpty(userInput.text))
                return;
            
            OnUserMessageSent?.Invoke(_messageReplyToId, userInput.text);
            
            userInput.text = string.Empty;
            UpdateReplyToId();
        }
        
        public void OnDataReceived(ChatMessage chatMessage, ChatMessage linkedMessage)
        {
            Debug.Log("UI processing new message");
            UpdateChatHistory(chatMessage, linkedMessage);
            StartCoroutine(UpdateScroll());
        }

        public void ResetChatHistory()
        {
            //chatContent.removeAllChildren();
        }
        
        public void OnCancelReplyToButtonClick()
        {
            UpdateReplyToId();
        }
        
        private void UpdateChatHistory(ChatMessage chatMessage, ChatMessage linkedMessage)
        {
            Debug.Log($"Updating Chat history with new message: {chatMessage}");
            //chatHistory.text += message + Environment.NewLine;
            
            // Instantiate new message bubble and populate
            var newMessage = Instantiate(chatMessagePrefab, chatContent);
            var messageManager = newMessage.GetComponent<ChatMessageUIManager>();
            messageManager.SetupChatMessage(chatMessage, linkedMessage, UpdateReplyToId);
        }

        private IEnumerator UpdateScroll()
        {
            // Wait for next frame so that everything is loaded before updating the scroll
            yield return null;
            Debug.Log("Updating Chat history scroll");
            Canvas.ForceUpdateCanvases();
            chatHistoryScrollRect.verticalNormalizedPosition = 0f;
        }

        // Includes default parameters for reset
        private void UpdateReplyToId(int replyToId = 0, string username = "", string message = "")
        {
            Debug.Log($"Update Reply to Message toggled for id { replyToId }");
            _messageReplyToId = replyToId;
            
            // Show/hide replying to preview above input field
            if (_messageReplyToId == 0)
            {
                replyingToPanel.SetActive(false);
                return;
            }
            
            // Update preview variables
            replyingToUserName.text = username;
            replyingToMessageText.text = message;
            
            // Enable again
            replyingToPanel.SetActive(true);
        }
        
        private void ValidateDependencies()
        {
            enabled = IsDependencyConfigured(name, "User Input", userInput) &&
                      IsDependencyConfigured(name, "User List", usersList);
        }    
    }
}

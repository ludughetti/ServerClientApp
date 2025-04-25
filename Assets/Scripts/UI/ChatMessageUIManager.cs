using System;
using Messages;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ChatMessageUIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text userName;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private GameObject repliedToObject;
        [SerializeField] private TMP_Text replyToUserName;
        [SerializeField] private TMP_Text replyToMessageText;

        private ChatMessage _message;
        private Action<int, string, string> _onReplyToChanged;
        
        public void SetupChatMessage(ChatMessage message, ChatMessage linkedMessage, 
            Action<int, string, string> replyToButtonCallback)
        {
            _message = message;
            _onReplyToChanged = replyToButtonCallback;
            
            //Setup user and message info
            userName.text = message.GetUsername(); 
            messageText.text = message.GetMessage();

            // If it's not a reply to another message, early exit
            if (linkedMessage == null) return;
            
            // Else, setup replied to message info
            repliedToObject.SetActive(true);
            replyToUserName.text = linkedMessage.GetUsername();
            replyToMessageText.text = linkedMessage.GetMessage();
        }
        
        public void OnReplyToButtonClick()
        {
            Debug.Log($"Next message will reply to message { _message.GetId() }");
            _onReplyToChanged?.Invoke(_message.GetId(), _message.GetUsername(), _message.GetMessage());
        }
    }
}

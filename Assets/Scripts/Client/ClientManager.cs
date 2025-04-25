using System;
using System.Collections.Generic;
using System.Net;
using Messages;
using UnityEngine;

namespace Client
{
    public abstract class ClientManager
    {
        protected string _onConnectMessage = "User connected to chat";
        protected string _username;
        
        private Queue<ChatMessage> _uiPendingMessages = new();
        private List<ChatMessage> _chatHistory = new ();
        
        public Action<ChatMessage, ChatMessage> OnDataReceived;

        public abstract void StartClient(IPAddress ipAddress, int port);
        public abstract void CloseClient();
        public abstract void SendDataToServer(int linkedMessageId, string message);
        public abstract void OnRead(IAsyncResult result);

        public void FlushQueuedMessages()
        {
            while (_uiPendingMessages.Count > 0)
            {
                Debug.Log("Processing client UI pending messages");
                var message = _uiPendingMessages.Dequeue();
                ChatMessage linkedMessage = null;

                // Fetch the linked message
                if (message.GetLinkedMessageId() != 0)
                    linkedMessage = _chatHistory.Find(chatMessage => message.GetLinkedMessageId() == chatMessage.GetId());
                
                OnDataReceived?.Invoke(message, linkedMessage);
            }
        }
        
        protected void StoreNewMessage(byte[] data)
        {
            var chatMessage = ChatMessage.DecodeMessage(data);
            
            // Assign Id depending on history size
            chatMessage.SetId(_chatHistory.Count + 1);
            
            // Add messages to history
            _chatHistory.Add(chatMessage);
            
            // Queue in pending messages
            _uiPendingMessages.Enqueue(chatMessage);
            
            Debug.Log($"Processed message: { chatMessage }");
        }
    }
}

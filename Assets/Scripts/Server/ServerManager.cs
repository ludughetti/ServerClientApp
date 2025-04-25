using System;
using System.Collections.Generic;
using Messages;
using UnityEngine;

namespace Server
{
    public abstract class ServerManager : MonoBehaviour
    {
        protected bool _queueUIPendingMessages;
        
        private Queue<ChatMessage> _uiPendingMessages = new ();
        private List<ChatMessage> _chatHistory = new ();
        
        public Action<ChatMessage, ChatMessage> OnDataReceived;
        
        public abstract void StartServer(int portNumber, bool queueUIPendingMessages);
        public abstract void StopServer();
        public abstract void ReceiveAndBroadcastData(byte[] data);
        public abstract void OnRead(IAsyncResult result);

        public void FlushEnqueuedMessages()
        {
            while (_uiPendingMessages.Count > 0)
            {
                Debug.Log("Processing server UI pending messages");
                var message = _uiPendingMessages.Dequeue();
                ChatMessage linkedMessage = null;
                
                // Fetch the linked message
                if (message.GetLinkedMessageId() != 0)
                    linkedMessage = _chatHistory.Find(chatMessage => message.GetLinkedMessageId() == chatMessage.GetId());
                
                OnDataReceived?.Invoke(message, linkedMessage);
            }
        }
        
        protected ChatMessage StoreNewMessage(byte[] data)
        {
            var chatMessage = ChatMessage.DecodeMessage(data);
            
            // Assign Id depending on history size
            chatMessage.SetId(_chatHistory.Count + 1);
            
            // Add messages to history
            _chatHistory.Add(chatMessage);
            
            // Queue in pending messages if it's running as server only so that UI is updated
            if (_queueUIPendingMessages)
                _uiPendingMessages.Enqueue(chatMessage);
            
            return chatMessage;
        }
    }
}

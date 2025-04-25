using System;
using System.Collections.Generic;
using Messages;
using UnityEngine;
using static Utils.Encoder;

namespace Server
{
    public abstract class ServerManager : MonoBehaviour
    {
        protected Queue<ChatMessage> _uiPendingMessages = new ();
        protected Queue<ChatMessage> _chatHistory = new ();
        protected bool _queueUIPendingMessages;
        
        public Action<ChatMessage> OnDataReceived;
        
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
                
                OnDataReceived?.Invoke(message);
            }
        }
        
        protected ChatMessage QueueNewMessage(byte[] data)
        {
            var chatMessage = ChatMessage.DecodeMessage(data);
            
            // Queue messages in history
            _chatHistory.Enqueue(chatMessage);
            
            // Queue in pending messages if it's running as server only so that UI is updated
            if (_queueUIPendingMessages)
                _uiPendingMessages.Enqueue(chatMessage);
            
            return chatMessage;
        }
    }
}

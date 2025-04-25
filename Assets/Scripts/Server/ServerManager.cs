using System;
using System.Collections.Generic;
using UnityEngine;
using static Utils.Encoder;

namespace Server
{
    public abstract class ServerManager : MonoBehaviour
    {
        protected Queue<string> _uiPendingMessages = new ();
        protected Queue<string> _chatHistory = new ();
        protected bool _queueUIPendingMessages;
        
        public Action<byte[]> OnDataReceived;
        
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
                
                OnDataReceived?.Invoke(Encode(message));
            }
        }
    }
}

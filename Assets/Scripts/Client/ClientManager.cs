using System;
using System.Collections.Generic;
using System.Net;
using Messages;
using UnityEngine;

namespace Client
{
    public abstract class ClientManager
    {
        protected Queue<ChatMessage> _dataReceived = new();
        protected string _onConnectMessage = "User connected to chat";
        protected string _username;
        public Action<ChatMessage> OnDataReceived;

        public abstract void StartClient(IPAddress ipAddress, int port);
        public abstract void CloseClient();
        public abstract void SendDataToServer(string message);
        public abstract void OnRead(IAsyncResult result);

        public void FlushQueuedMessages()
        {
            while (_dataReceived.Count > 0)
            {
                Debug.Log("Processing client UI pending messages");
                var data = _dataReceived.Dequeue();
                try
                {
                    OnDataReceived.Invoke(data);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}

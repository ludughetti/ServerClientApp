using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Client
{
    public abstract class ClientManager
    {
        protected Queue<byte[]> _dataReceived = new();
        protected string _onConnectMessage = "User connected to chat";
        public Action<byte[]> OnDataReceived;

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

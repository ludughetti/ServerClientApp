using System;
using System.Text;

namespace Messages
{
    public class ChatMessage
    {
        private string _username;
        private string _message;

        public ChatMessage(string username, string message)
        {
            this._username = username;
            this._message = message;
        }

        public string GetUsername()
        {
            return this._username;
        }

        public string GetMessage()
        {
            return this._message;
        }

        public byte[] EncodeMessage()
        {
            var usernameBytes = Encoding.UTF8.GetBytes(_username);
            var messageBytes = Encoding.UTF8.GetBytes(_message);

            var result = new byte[1 + usernameBytes.Length + messageBytes.Length];
            result[0] = (byte)usernameBytes.Length;

            Buffer.BlockCopy(usernameBytes, 0, result, 1, usernameBytes.Length);
            Buffer.BlockCopy(messageBytes, 0, result, 1 + usernameBytes.Length, messageBytes.Length);

            return result;
        }

        public static ChatMessage DecodeMessage(byte[] data)
        {
            int usernameLength = data[0];
            var username = Encoding.UTF8.GetString(data, 1, usernameLength);
            var message = Encoding.UTF8.GetString(data, 1 + usernameLength, data.Length - 1 - usernameLength);
            
            return new ChatMessage(username, message);
        }

        public override string ToString()
        {
            return _username + " - " + _message;
        }
    }
}

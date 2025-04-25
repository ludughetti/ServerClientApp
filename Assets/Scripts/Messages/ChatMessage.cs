using System;
using System.Text;

namespace Messages
{
    public class ChatMessage
    {
        private int _id;
        private int _linkedMessageId;
        private string _username;
        private string _message;

        public ChatMessage(int id, int linkedMessageId, string username, string message)
        {
            _id = id;
            _linkedMessageId = linkedMessageId;
            _username = username;
            _message = message;
        }

        public int GetId()
        {
            return _id;
        }

        public void SetId(int newId)
        {
            _id = newId;
        }
        
        public int GetLinkedMessageId()
        {
            return _linkedMessageId;
        }

        public string GetUsername()
        {
            return _username;
        }

        public string GetMessage()
        {
            return _message;
        }

        // Format the ChatMessage object into a string using | as a delimeter
        // This string will then be encoded to byte[]
        public byte[] EncodeMessage()
        {
            var formatted = $"{_id}|{_linkedMessageId}|{_username}|{_message}";
            return Encoding.UTF8.GetBytes(formatted);
        }

        // Decode the byte[] into a string
        // This string should contain the ChatMessage information with each field separated by a | 
        public static ChatMessage DecodeMessage(byte[] data)
        {
            var decoded = Encoding.UTF8.GetString(data);
            var parts = decoded.Split('|');

            if (parts.Length < 4)
                throw new Exception("Invalid message format");

            var id = int.Parse(parts[0]);
            var linkedId = int.Parse(parts[1]);
            var username = parts[2];
            var message = parts[3];

            return new ChatMessage(id, linkedId, username, message);
        }

        public override string ToString()
        {
            return _username + " - " + _message;
        }
    }
}

using System.Text;

namespace Utils
{
    public static class Encoder
    {
        public static string Decode(byte[] data) {
            return Encoding.UTF8.GetString(data);
        }
        
        public static byte[] Encode(string data) {
            return Encoding.UTF8.GetBytes(data);
        }
    }
}

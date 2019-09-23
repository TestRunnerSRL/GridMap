using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridMap
{
    public class Message
    {
        public enum Type { CLOSE, PING, PONG, CONFIG, TEXT, DATA };

        public Type type;
        public string sender;
        public byte[] data;
        public string DataString
        {
            get
            {
                return ASCIIEncoding.ASCII.GetString(data);
            }
            set
            {
                data = ASCIIEncoding.ASCII.GetBytes(value);
            }
        }

        internal byte[] GetRaw()
        {
            var senderBytes = ASCIIEncoding.ASCII.GetBytes(sender);
            byte senderLength = (byte)senderBytes.Length;

            int length = 1 + senderBytes.Length + data.Length + 1 + 4;
            byte[] lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);

            var bytes = new byte[length];
            int bytesOffset = 0;

            lengthBytes.CopyTo(bytes, bytesOffset);
            bytesOffset += 4;
            bytes[bytesOffset] = (byte)type;
            bytesOffset++;
            bytes[bytesOffset] = senderLength;
            bytesOffset++;
            senderBytes.CopyTo(bytes, bytesOffset);
            bytesOffset += senderBytes.Length;
            data.CopyTo(bytes, bytesOffset);

            return bytes;
        }

        internal static Message ParseRaw(List<byte> receiveObjectBytes, int receiveLength, out int length)
        {
            length = 0;
            if (receiveLength < 6)
            {
                return null;
            }

            byte[] lengthBytes = receiveObjectBytes.Take(4).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);
            length = BitConverter.ToInt32(lengthBytes, 0);

            if (receiveLength < length)
            {
                return null;
            }

            byte[] receiveBytes = receiveObjectBytes.Take(length).ToArray();
            byte senderLength = receiveBytes[5];

            byte[] receiveData = new byte[length - (6 + senderLength)];
            Array.Copy(receiveBytes, (6 + senderLength), receiveData, 0, length - (6 + senderLength));

            return new Message(
                (Type)receiveBytes[4], receiveData,
                ASCIIEncoding.ASCII.GetString(receiveBytes, 6, senderLength));
        }

        public Message(Type type, byte[] data, string sender = "")
        {
            this.type = type;
            this.sender = sender;
            this.data = data;
        }

        public Message(Type type, string data, string sender = "")
        {
            this.type = type;
            this.sender = sender;
            this.DataString = data;
        }
    }

}

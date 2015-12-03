using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace SecureCalendarLib
{
    [Serializable]
    public class Ack { }

    /*
        LOGIN Messsages
    */
    [Serializable]
    public class LoginRequest
    {
        public string username = "";
    }

    [Serializable]
    public class LoginChallenge
    {
        public string passwordSalt = "";
    }

    [Serializable]
    public class LoginResponse
    {
        public string passwordHash = "";
    }
    [Serializable]
    public class LoginConfirmation
    {
        //encrypted using PBKDF2(pw,simmetricSalt)
        public string encryptedPrivateKey = "";
        public string privateIV = "";
        public List<UserPublicKey> permission = new List<UserPublicKey>();
    }
    [Serializable]
    public class UserPublicKey
    {
        public string username;
        public string publicKey;
    }

    /*
        end of LOGIN messages
    */
    [Serializable]
    public class ReadCalendarRequest
    {
        public string calendarName = "";
    }

    [Serializable]
    public class SecureCalendar
    {
        //Base64 of calendar xml encrypted with use FEK
        public string name = "Business Calendar";
        public string IV = "";
        public string encryptedEvents = "";
        // Maps a user to the encrypted FEK(file encryption key) using the user private key
        // dictionary keys contain the usernames of the users with permission to access the caleendar
        public List<EncryptedFileEncryptionKey> keys = new List<EncryptedFileEncryptionKey>();
    }
    [Serializable]
    public class EncryptedFileEncryptionKey
    {
        public string username;
        public string eFEK;
    }

    public class Util
    {
        public static void writeObject(SslStream sslStream, object obj)
        {
            byte[] userDataBytes;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, obj);
            userDataBytes = ms.ToArray();
            // lenght header
            byte[] userDataLen = BitConverter.GetBytes((Int32)userDataBytes.Length);
            sslStream.Write(userDataLen, 0, 4);
            sslStream.Write(userDataBytes, 0, userDataBytes.Length);
        }

        public static object readObject(SslStream sslStream)
        {
            byte[] readMsgLen = new byte[4];
            int dataRead = 0;
            do
            {
                dataRead += sslStream.Read(readMsgLen, 0, 4 - dataRead);
            } while (dataRead < 4);

            int dataLen = BitConverter.ToInt32(readMsgLen, 0);
            if (dataLen == 0)
            {
                return null;
            }
            byte[] readMsgData = new byte[dataLen];

            int len = dataLen;
            dataRead = 0;
            do
            {
                dataRead += sslStream.Read(readMsgData, dataRead, len - dataRead);

            } while (dataRead < len);
            //deserialize            
            MemoryStream ms = new MemoryStream(readMsgData);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms.Position = 0;
            object rawObj = bf1.Deserialize(ms);            
            return rawObj;
        }


        public static string XmlSerializeToString(object output)
        {
            var serializer = new XmlSerializer(output.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, output);
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }


    }
    /*
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
    */
}

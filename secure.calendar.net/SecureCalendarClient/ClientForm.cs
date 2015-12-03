using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading;
using SecureCalendarLib;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;


/*
    Install generated server certificate "server.crt" (see server.cs file):
    > certutil -addstore "Root" "server.crt"

    check installed running
    > mmc
    In the GUI, File Menu, Add/Remove snap in, add Certificates, press ok

*/

namespace SecureCalendarClient
{
    public partial class ClientForm : Form
    {
        const int SALT_SIZE = 16;
        const int KEY_SIZE = 16;
        const int ITERATIONS = 10000;
        const int RSA_SIZE = 2048;

        string HOST = "127.0.0.1";
        int PORT = 4321;
        string CN = "Secure Calendar";

        public ClientForm()
        {
            InitializeComponent();
            Thread t = new Thread(new ThreadStart(connect));
            t.Start();
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        private void connect()
        {
            // Create a TCP/IP client socket.
            // machineName is the host running the server application.
            TcpClient client = new TcpClient(HOST, PORT);

            log("Client connected.");
            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(
               client.GetStream(),
               false,
               new RemoteCertificateValidationCallback(ValidateServerCertificate),
               null
               );
            try
            {               
                // The server name must match the name on the server certificate.
                sslStream.AuthenticateAsClient(CN);
                log("Server authenticated");
                protocol(sslStream);
                log("protocol finished. Exit...");
            }
            catch (AuthenticationException e)
            {
                logF("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    logF("Inner exception: {0}", e.InnerException.Message);
                }
                logF("Authentication failed - closing the connection.");
                client.Close();
                return;
            }
            finally
            {
                sslStream.Close();
                client.Close();
            }            
        }

        private void protocol(SslStream sslStream)
        {
            var username = "rui";
            var password = "ruisirs";
            object recvObj = null;
            var request = new LoginRequest() { username = username};
            LoginChallenge challenge = null;
            LoginResponse response = null;
            LoginConfirmation confirmation = null;
            SecureCalendar sc = null;

            log(Util.XmlSerializeToString(request));
            Util.writeObject(sslStream, request);
            recvObj = Util.readObject(sslStream);
            if(recvObj is LoginChallenge)
            {
                challenge = (LoginChallenge)recvObj;
                log(Util.XmlSerializeToString(challenge));
            }
            else
            {
                log("Login failed: did not receive login challenge");
                return;
            }
            // PBKDF2
            string pwhash = "";
            // password hash
            using (var db = new Rfc2898DeriveBytes(password,
                Convert.FromBase64String(challenge.passwordSalt),
                ITERATIONS))
            {                
                pwhash = Convert.ToBase64String(db.GetBytes(KEY_SIZE));
            }
            response = new LoginResponse() { passwordHash = pwhash };
            Util.writeObject(sslStream,response);
            log(Util.XmlSerializeToString(response));
            recvObj = Util.readObject(sslStream);
            
            if (recvObj is LoginConfirmation)
            {
                confirmation = (LoginConfirmation)recvObj;
                log(Util.XmlSerializeToString(confirmation));
            }
            else
            {
                log("Login failed: did not receive login confirmation");
                return;
            }
            log("Login succesfull");
            /*
            ReadCalendarRequest read = new ReadCalendarRequest() { calendarName = "Rui's calendar" };
            Util.writeObject(sslStream,read);
            recvObj = Util.readObject(sslStream);
            if(recvObj is SecureCalendar)
            {
                sc = (SecureCalendar)recvObj;
            }
            else
            {
                log("Read failed");
                return;
            }
            log("Read successfull");
            */
        }
        /*
        private void writeObject(SslStream sslStream, object obj)
        {
            byte[] userDataBytes;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, obj);
            userDataBytes = ms.ToArray();
            byte[] userDataLen = BitConverter.GetBytes((Int32)userDataBytes.Length);
            sslStream.Write(userDataLen, 0, 4);
            sslStream.Write(userDataBytes, 0, userDataBytes.Length);
            logF("Sent an object of type {0} length {1}:", obj.GetType(), userDataBytes.Length);
            log(Util.XmlSerializeToString(obj));
        }

        private object readObject(SslStream sslStream)
        {
            byte[] readMsgLen = new byte[4];
            int dataRead = 0;
            do
            {
                dataRead += sslStream.Read(readMsgLen, 0, 4 - dataRead);
            } while (dataRead < 4);

            int dataLen = BitConverter.ToInt32(readMsgLen, 0);
            logF("header: message length {0}", dataLen);
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
            logF("received {0} bytes", len);
            //deserialize
            MemoryStream ms = new MemoryStream(readMsgData);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms.Position = 0;
            object rawObj = bf1.Deserialize(ms);
            log(Util.XmlSerializeToString(rawObj));
            return rawObj;
        }

    */












































        private delegate void logDelegate(string str);
        public void log(string str)
        {
            if (InvokeRequired)
            {
                Invoke(new logDelegate(log), new object[] { str });
            }
            else
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException("lol?");
                }
                logForm.AppendText(str + "\r\n\r\n");
            }

        }
        public void logF(string fmt, params object[] args)
        {
            log(string.Format(fmt, args));
        }
    }
}

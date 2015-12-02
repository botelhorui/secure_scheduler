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
            SecureCalendar sc = new SecureCalendar()
            {
                name = "New client calendar"
            };
            sc.events.Add("12:00 Lunch");
            sc.events.Add("14:00 Sleep");
            log("Created calendar:");
            log(Util.ObjectToXml(sc));
            writeObject(sslStream, sc);
        }

        private void writeObject(SslStream sslStream, SecureCalendar sc)
        {
            byte[] userDataBytes;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, sc);
            userDataBytes = ms.ToArray();
            byte[] userDataLen = BitConverter.GetBytes((Int32)userDataBytes.Length);
            sslStream.Write(userDataLen, 0, 4);
            sslStream.Write(userDataBytes, 0, userDataBytes.Length);
            logF("Sent an object of type {0} length {1}", sc.GetType(), userDataBytes.Length);
        }

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
                logForm.AppendText(str + "\r\n");
            }

        }
        public void logF(string fmt, params object[] args)
        {
            log(string.Format(fmt, args));
        }
    }
}

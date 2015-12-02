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
using System.Runtime.Serialization.Formatters.Binary;
using SecureCalendarLib;

/*
http://www.codeproject.com/Articles/162194/Certificates-to-DB-and-Back

Generation of 2048 bits, RSA private key.
> openssl genrsa -out private.key

Generate public key.
> openssl rsa -in private.key -puout > public.key

Generate a self signed certificate. To be used as CA certificate.
request:
> openssl req -new -key private.key -out server.csr
self sign:
> openssl x509 -req -days 365 -in server.csr -signkey private.key -out server.crt



store private key next to certificate, pkcs12 format
> openssl pkcs12 -export -in server.crt -inkey private.key -out cert_key.p12
> password:sirs

*/

namespace SecureCalendarServer
{
    public partial class ServerForm : Form
    {
        private object certificateLock = new object();
        private X509Certificate2 certificate;
        private object listenLock = new object();
        public ServerForm()
        {
            InitializeComponent();
            certificate = new X509Certificate2("cert_key.p12", "sirs");

            Thread t = new Thread(new ThreadStart(listen));            
            t.Start();
            
        }

        private void listen()
        {
            log(string.Format("Started listen thread:{0}", Thread.CurrentThread.ManagedThreadId));
            TcpListener listener = new TcpListener(IPAddress.Any, 4321);
            listener.Start();
            lock (listenLock)
            {
                while (true)
                {
                    log("Waiting for a client to connect...");                
                    TcpClient client = listener.AcceptTcpClient();
                    log("Received a connection");
                    process(client);
                }

            }
        }

        private void process(TcpClient client)
        {
            // A client has connected. Create the 
            // SslStream using the client's network stream.
            SslStream sslStream = new SslStream(
                client.GetStream(), false);
            // Authenticate the server but don't require the client to authenticate.
            try
            {
                lock (certificateLock)
                {
                    sslStream.AuthenticateAsServer(certificate,false, SslProtocols.Tls, true);
                }
                log("Server authentication complete");
                protocol(sslStream);
                log("Finished protocol, exit...");
                      
            }
            catch (AuthenticationException e)
            {
                logF("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    logF("Inner exception: {0}", e.InnerException.Message);
                }
                logF("Authentication failed - closing the connection.");
                sslStream.Close();
                client.Close();
                return;
            }
            finally
            {
                // The client stream will be closed with the sslStream
                // because we specified this behavior when creating
                // the sslStream.
                sslStream.Close();
                client.Close();
            }
        }

        private void protocol(SslStream sslStream)
        {
            //sslStream.ReadTimeout = 5000;
            //sslStream.WriteTimeout = 5000;
            while (true)
            {
                object req = readObject(sslStream);
                if(req is SecureCalendar)
                {
                    var sc = (SecureCalendar)req;
                    log(Util.ObjectToXml(sc));
                }else if (req == null)
                {

                }
                else
                {

                }
                //object response = null;
                //writeObject(sslStream, response);
            }
        }

        private void writeObject(SslStream sslStream, object response)
        {
            throw new NotImplementedException();
        }

        private object readObject(SslStream sslStream)
        {
            byte[] readMsgLen = new byte[4];            
            int dataRead = 0;
            do
            {
                dataRead += sslStream.Read(readMsgLen, 0, 4 - dataRead);
            } while (dataRead < 4);

            int dataLen = BitConverter.ToInt32(readMsgLen,0);
            logF("header: message length {0}", dataLen);
            if(dataLen == 0)
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
            logF("received {0} bytes", len) ;
            //deserialize
            MemoryStream ms = new MemoryStream(readMsgData);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms.Position = 0;
            object rawObj = bf1.Deserialize(ms);
            return rawObj;
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


        private void logForm_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

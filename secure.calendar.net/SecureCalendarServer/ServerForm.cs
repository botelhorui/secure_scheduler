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
using System.Security.Cryptography;

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
    [Serializable]
    public class UserData
    {
        public string username;

        public string passwordSalt;
        public string passwordHash;

        public string KEKSalt;
                
        public string publicKey;
        public string privateIV;
        public string encryptedPrivateKey;
    }

    [Serializable]
    public class ServerData
    {
        public List<UserData> users = new List<UserData>();
        public List<SecureCalendar> calendars = new List<SecureCalendar>();
    }
    
    public partial class ServerForm : Form
    {
        const int SALT_SIZE = 16;
        const int KEY_SIZE = 16;
        const int ITERATIONS = 10000;
        const int RSA_SIZE = 2048;

        private object certificateLock = new object();
        private X509Certificate2 certificate;
        private object listenLock = new object();

        private ServerData serverData = null;

        //Data 

        public ServerForm()
        {
            InitializeComponent();
            //generateInitialState();
            //return;
            
            var xml = File.ReadAllText("initial.state.xml");
            logF("file loaded {0} length", xml.Length);
            serverData = Util.XmlDeserializeFromString<ServerData>(xml);
            log(Util.XmlSerializeToString(serverData));
            certificate = new X509Certificate2("cert_key.p12", "sirs");
            
            Thread t = new Thread(new ThreadStart(listen));            
            t.Start();
        }

        public void generateInitialState()
        {
            
            string u1 = "rui";
            string pw1 = "ruisirs";
            string c1 = "Rui's calendar";
            UserData ud1 = registerUser(u1, pw1);
            SecureCalendar sc1 = registerCalendar(u1, pw1, ud1);
            sc1.name = c1;
            string u2 = "ricardo";
            string pw2 = "ricardoinesc";
            string c2 = "Ricardo's calendar";
            UserData ud2 = registerUser(u2, pw2);
            SecureCalendar sc2 = registerCalendar(u2, pw2, ud2);
            ServerData sd = new ServerData() { };
            sd.calendars.Add(sc1);
            sd.calendars.Add(sc2);
            sd.users.Add(ud1);
            sd.users.Add(ud2);
            serverData = sd;
            log("Initial State generated");
            log(Util.XmlSerializeToString(sd));
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

            /*
                User authentication
            */
            UserData userData = null;
            object req = null;
            req = Util.readObject(sslStream);
            if (req is LoginRequest)
            {
                var lr = (LoginRequest)req;
                log(Util.XmlSerializeToString(lr));
                userData = serverData.users.Find(x => x.username == lr.username);
                if (userData==null)
                {
                    log("Login failed: unknown user");
                    return;
                }
                LoginChallenge lc = new LoginChallenge()
                {
                    passwordSalt = userData.passwordSalt
                };
                Util.writeObject(sslStream, lc);
                log(Util.XmlSerializeToString(lc));
            }
            else
            {
                log("Login failed: did not receive loginRequest");
                return;
            }           

            req = Util.readObject(sslStream);
            if (req is LoginResponse)
            {
                var lr = (LoginResponse)req;
                log(Util.XmlSerializeToString(lr));
                if (lr.passwordHash != userData.passwordHash)
                {
                    log("Login failed: different password hash");
                }
                var lc = new LoginConfirmation()
                {
                    encryptedPrivateKey = userData.encryptedPrivateKey,
                    privateIV = userData.privateIV,
                    KEKSalt = userData.KEKSalt
                };
                foreach(var x in serverData.users)
                {
                    lc.permission.Add(new UserPublicKey()
                    { username = x.username, publicKey = x.publicKey});
                }
                Util.writeObject(sslStream, lc);
                log(Util.XmlSerializeToString(lc));
            }
            else
            {
                log("Login failed: did not receive loginRequest");
                return;
            }
            log("Login successfull");
            /*
                User is now authenticated.
            */
            while (true)
            {
                req = Util.readObject(sslStream);
                if(req is ReadCalendarRequest)
                {
                    ReadCalendarRequest read = (ReadCalendarRequest)req;
                    var calendarName = read.calendarName;
                    SecureCalendar sc = serverData.calendars.Find(c => c.name == calendarName);
                    if(sc == null)
                    {
                        log("Read calendar invalid");
                        return;
                    }
                    var efek = sc.keys.Find(x => x.username == userData.username);
                    if(efek == null)
                    {
                        log("Read calendar invlalid");
                        return;
                    }
                    Util.writeObject(sslStream, sc);
                    log("Read successfull");

                }else if(req is SecureCalendar)
                {
                    SecureCalendar newCalendar = (SecureCalendar)req;
                    SecureCalendar oldCalendar = serverData.calendars.Find(c => c.name == newCalendar.name);
                    if (oldCalendar == null)
                    {
                        log("Write calendar invalid");
                        return;
                    }
                    var efek = oldCalendar.keys.Find(x => x.username == userData.username);
                    if (efek == null)
                    {
                        log("Read calendar invlalid");
                        return;
                    }
                    serverData.calendars.Add(newCalendar);
                    serverData.calendars.Remove(oldCalendar);
                    log("Write successfull");
                    // TODO check if permitions where changed
                }
                else
                {
                    var m = "null";
                    if (req != null)
                        m = req.GetType().ToString();
                    logF("Protocol failed: received unexpected message type:{0}", m);
                }                              
            }
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
            log(Util.XmlSerializeToString(rawObj));
            return rawObj;
        }*/























        public SecureCalendar registerCalendar(string username, string password, UserData userdata)
        {
            SecureCalendar sc = new SecureCalendar() { };

            // KEK
            string FEK = "";
            string events = "10:00 Dev team meeting. 20:00 Son birthday party";

            // kek generation
            using (var db = new Rfc2898DeriveBytes(password, SALT_SIZE, ITERATIONS))
            {
                db.Salt = Convert.FromBase64String(userdata.KEKSalt);
                FEK = Convert.ToBase64String(db.GetBytes(KEY_SIZE));
            }
            // generate KEK
            using (var cipher = new AesManaged())
            {
                cipher.Mode = CipherMode.CBC;
                cipher.KeySize = KEY_SIZE * 8;
                cipher.GenerateKey();
                cipher.GenerateIV();
                FEK = Convert.ToBase64String(cipher.Key);
                sc.IV = Convert.ToBase64String(cipher.IV);
                using (ICryptoTransform encryptor = cipher.CreateEncryptor(
                    cipher.Key,
                    cipher.IV))
                {
                    using (MemoryStream to = new MemoryStream())
                    {
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            byte[] x = Encoding.UTF8.GetBytes(Util.XmlSerializeToString(events));
                            writer.Write(x, 0, x.Length);
                            writer.FlushFinalBlock();
                            sc.encryptedEvents = Convert.ToBase64String(to.ToArray());
                        }
                    }
                }
                cipher.Clear();
            }
            //Encode FEK with public key
            RSACryptoServiceProvider rsaPublic = new RSACryptoServiceProvider();
            rsaPublic.FromXmlString(userdata.publicKey);
            byte[] eFEKb = rsaPublic.Encrypt(Convert.FromBase64String(FEK), false);
            string eFEK = Convert.ToBase64String(eFEKb);
            sc.keys.Add(new EncryptedFileEncryptionKey() {
                username = username,
                eFEK = eFEK
            });
            return sc;
        }
        public UserData registerUser(string username, string password)
        {
            // PBKDF2
            string pwsalt = "";
            string pwhash = "";
            // password hash
            using (var db = new Rfc2898DeriveBytes(password, SALT_SIZE, ITERATIONS))
            {
                pwsalt = Convert.ToBase64String(db.Salt);
                pwhash = Convert.ToBase64String(db.GetBytes(KEY_SIZE));

            }
            // KEK
            string keksalt = "";
            string kek = "";
            // kek generation
            using (var db = new Rfc2898DeriveBytes(password, SALT_SIZE, ITERATIONS))
            {
                keksalt = Convert.ToBase64String(db.Salt);
                kek = Convert.ToBase64String(db.GetBytes(KEY_SIZE));
            }
            // RSA
            string publickey;
            string privatekey;
            string privateiv;
            string encryptedprivatekey;
            // Generate RSA key pair
            using (var rsa = new RSACryptoServiceProvider(RSA_SIZE))
            {
                try
                {                    
                    privatekey = rsa.ToXmlString(true);
                    publickey = rsa.ToXmlString(false);
                }
                finally
                {
                    // IMPORTANT, avoid storing key in windows store
                    rsa.PersistKeyInCsp = false;
                }
            }
            byte[] privatekeyb = Encoding.UTF8.GetBytes(privatekey);

            /*
            logF("register user {0} , password {1}", username, password);
            log("");
            logF("kek {0}", kek);
            log("");
            logF("privatekey {0}", privatekey);
            log("");
            */
            using (var cipher = new AesManaged())
            {
                cipher.Mode = CipherMode.CBC;
                cipher.KeySize = 128;
                cipher.BlockSize = 128;
                cipher.Padding = PaddingMode.PKCS7;
                //
                cipher.GenerateIV();
                privateiv = Convert.ToBase64String(cipher.IV);
                cipher.Key = Convert.FromBase64String(kek);
                cipher.IV = Convert.FromBase64String(privateiv);
                using (ICryptoTransform encryptor = cipher.CreateEncryptor(cipher.Key,cipher.IV))
                {
                    using (MemoryStream to = new MemoryStream())
                    {
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            
                            writer.Write(privatekeyb, 0, privatekeyb.Length);
                            writer.FlushFinalBlock();
                            encryptedprivatekey = Convert.ToBase64String(to.ToArray());
                        }
                    }
                }
               
                cipher.Clear();
            }

            UserData ud = new UserData()
            {
                username = username,
                passwordSalt = pwsalt,
                passwordHash = pwhash,
                KEKSalt = keksalt,
                privateIV = privateiv,
                publicKey = publickey,
                encryptedPrivateKey = encryptedprivatekey
            };
            return ud;

        }

        private delegate void logDelegate(string str);
        public void log(string str)
        {
            if (logForm.InvokeRequired)
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


        private void logForm_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

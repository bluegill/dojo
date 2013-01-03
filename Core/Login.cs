using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Dojo.Core
{
    class Login
    {
        /* Dojo | Club Penguin Emulator
         * Written by Seether (http://tiber.me/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public Thread TcpThread;
        public TcpListener TcpListener;
        public NetworkStream ClientStream;
        public String lkey;
        public int rndk;
        public byte[] BufferData = new byte[7560];
        public MySQL MySQL = Server.MySQL;
        public void Create()
        {
            try
            {
                if (Config.ServerHost == "localhost")
                    TcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), Config.LoginPort);
                else
                    TcpListener = new TcpListener(IPAddress.Parse(Config.ServerHost), Config.LoginPort);
                
                TcpListener.Start();
                TcpThread = new Thread(new ThreadStart(AcceptClient));
                TcpThread.Start();
                Logger.Write("Login server has been initialized!");
            }
            catch
            {
                Logger.Write("Could not start the login server? Port may be in use?");
            }
        }
        public void AcceptClient()
        {
            while (true)
            {
                try
                {
                    TcpClient Client = TcpListener.AcceptTcpClient();
                    Thread ClientThread = new Thread(new ParameterizedThreadStart(HandleConn));
                    ClientThread.Start(Client);
                }
                catch { }
            }
        }
        public void HandleConn(object client)
        {
            TcpClient TcpClient = (TcpClient)client;
            ClientStream = TcpClient.GetStream();
            int BytesRead = 0;
            while (true)
            {
                try
                {
                    BytesRead = ClientStream.Read(BufferData, 0, 7560);
                }
                catch
                {
                    try
                    {
                        TcpClient.Close();
                        ClientStream.Close();
                        Logger.Write("A user has disconnected!");
                    }
                    catch { }
                }

                if (BytesRead == 0)
                {
                    break;
                }

                ASCIIEncoding ASCIIEncoding = new ASCIIEncoding();
                String data = ASCIIEncoding.GetString(BufferData, 0, BytesRead);

                this.ParseXMLData(ClientStream, data);
            }
        }
        public void Send(NetworkStream ClientStream, string data)
        {
            if (Config.ServerDebug == "true")
            {
                Logger.Write("Sending: " + data);
            }
            ASCIIEncoding ASCIIEncoding = new ASCIIEncoding();
            byte[] buffer = ASCIIEncoding.GetBytes(data + "\0");
            ClientStream.Write(buffer, 0, buffer.Length);
            ClientStream.Flush();
        }

        public String LoginKey
        {
            get { return lkey; }
            set { lkey = value; }
        }

        public void ParseXMLData(NetworkStream ClientStream, String data)
        {
            if (data.Contains("<policy-file-request/>"))
            {
                this.Send(this.ClientStream, "<cross-domain-policy><allow-access-from domain='*' to-ports='*' /></cross-domain-policy>");
            }
            else if (data.Contains("<msg t='sys'><body action='verChk' r='0'>"))
            {
                this.Send(this.ClientStream, "<msg t='sys'><body action='apiOK' r='0'></body></msg>");
            }
            else if (data.Contains("<msg t='sys'><body action='rndK' r='-1'></body></msg>"))
            {
                rndk = Helper.IntRandom(10000, 999999999);
                this.Send(this.ClientStream, "<msg t='sys'><body action='rndK' r='-1'><k>" + rndk + "</k></body></msg>");
            }
            else if (data.Contains("<msg t='sys'><body action='login' r='0'><login z="))
            {
                HandleJoinLogin(data);
            }
        }
        public void HandleJoinLogin(string data)
        {
            string user = Helper.StringBet(data, "<nick><![CDATA[", "]]").ToString();
            string pass = Helper.StringBet(data, "<pword><![CDATA[", "]]").ToString();

            string tempPass = null;
            int tempId = 0;

            MySqlDataReader reader = MySQL.GetData("SELECT * FROM users WHERE username='" + Helper.MySqlEscape(user) + "'");
            while (reader.Read())
            {
                tempId = Convert.ToInt32(reader["id"]);
                tempPass = reader["password"].ToString();
            }
            reader.Close();

            if (MySQL.CheckUser(user))
            {
                if (pass == Helper.HashPass(tempPass, rndk))
                {
                    LoginKey = Helper.GenerateLoginKey(rndk);
                    this.Send(this.ClientStream, "%xt%gs%-1%" + Core.ConstructServers() + "%");
                    this.Send(this.ClientStream, "%xt%l%-1%" + tempId + "%" + LoginKey + "%0%");
                }
                else
                {
                    this.Send(this.ClientStream, "%xt%e%-1%101%");
                }
            }
            else
            {
                this.Send(this.ClientStream, "%xt%e%-1%100%");
            }
        }
    }
}

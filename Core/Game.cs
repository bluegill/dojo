using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Dojo.Core
{
    class Game
    {
        /* Dojo | Club Penguin Emulator
         * Written by manipulate (http://tiber.me/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public TcpListener TcpListener;
        public Thread TcpThread;
        public List<User> Users = new List<User> { };
        public Login Login;
        public MySQL MySQL = Server.MySQL;
        public void Create(Login Login)
        {
            this.Login = Login;
            try
            {
                if (Config.ServerHost == "localhost")
                    this.TcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), Config.GamePort);
                else
                    this.TcpListener = new TcpListener(IPAddress.Parse(Config.ServerHost), Config.GamePort);
                
                TcpListener.Start();
                TcpThread = new Thread(new ThreadStart(AcceptClient));
                TcpThread.Start();
                Logger.Write("Game server has been initialized!");
            }
            catch (Exception e)
            {
                Logger.Write("Could not start the game server? Port may be in use: " + e.Message);
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
            List<TcpClient> TcpClients = new List<TcpClient> { };
            TcpClient TcpClient = (TcpClient)client;

            NetworkStream ClientStream = TcpClient.GetStream();

            int BytesRead = 0;
            byte[] BufferData = new byte[7560];

            while (true)
            {
                User User = new User(ClientStream, this);
                try
                {
                    BytesRead = ClientStream.Read(BufferData, 0, 7560);
                }
                catch
                {
                    try
                    {
                        foreach (User user in this.Users)
                        {
                            if (user.Connection == ClientStream)
                            {

                                this.Users.Remove(user);
                                TcpClient.Close();
                                user.Connection.Close();
                                ClientStream.Close();
                            }
                        }
                    }
                    catch { }
                }
                if (BytesRead == 0)
                {
                    try
                    {
                        foreach (User user in this.Users)
                        {
                            if (user.Connection == ClientStream)
                            {
                                try
                                {
                                    this.Users.Remove(user);
                                    TcpClient.Close();
                                    user.Connection.Close();
                                    ClientStream.Close();
                                    Logger.WriteNotice("A user has disconnected!");
                                }
                                catch { }
                            }
                        }
                        break;
                    }
                    catch { }
                }

                ASCIIEncoding ASCIIEncoding = new ASCIIEncoding();
                String data = ASCIIEncoding.GetString(BufferData, 0, BytesRead);

                if (!TcpClients.Contains((TcpClient)client))
                {
                    try
                    {
                        this.Users.Add(User);
                        TcpClients.Add((TcpClient)client);
                        Logger.WriteNotice("A user has connected!");
                    }
                    catch { }
                }

                try
                {
                    foreach (User user in this.Users)
                    {
                        if (user.Connection == ClientStream)
                        {
                            this.ProcessData(user, data);
                        }

                    }
                }
                catch { }
            }
        }
        public void ProcessData(User User, string data)
        {
            switch (data[0])
            {
                case '<':
                    ParseXMLData(User, data);
                    break;
                case '%':
                    ParseXtData(User, data);
                    break;
            }
        }
        public void ParseXtData(User User, string data)
        {
            string[] split = data.Split('%');
            string handle = split[2];
            switch (handle)
            {
                case "s":
                    ParseSysData(User, data);
                    break;
            }
        }
        public void ParseSysData(User User, string data)
        {
            string[] split = data.Split('%');
            string cmd = split[3];
            string[] handle = cmd.Split('#');
            switch (cmd)
            {
                case "i#ai":
                    HandleAddItem(User, data);
                    break;
            }
            switch (handle[0])
            {
                case "s":
                    HandleUser(User, data);
                    break;
                case "u":
                    HandleUser(User, data);
                    break;
                case "j":
                    HandleJoin(User, data);
                    break;
                case "i":
                    HandleInventory(User, data);
                    break;
                case "m":
                    HandleMessage(User, data);
                    break;
            }
        }
        public void ParseXMLData(User User, string data)
        {
            if (data.Contains("<policy-file-request/>"))
            {
                this.Send(User, "<cross-domain-policy><allow-access-from domain='*' to-ports='*' /></cross-domain-policy>");
            }
            else if (data.Contains("<msg t='sys'><body action='verChk' r='0'>"))
            {
                this.Send(User, "<msg t='sys'><body action='apiOK' r='0'></body></msg>");
            }
            else if (data.Contains("<msg t='sys'><body action='rndK' r='-1'></body></msg>"))
            {
                this.Send(User, "<msg t='sys'><body action='rndK' r='-1'><k>" + Login.rndk + "</k></body></msg>");
            }
            else if (data.Contains("<msg t='sys'><body action='login' r='0'><login z="))
            {
                HandleJoinLogin(User, data);
            }
        }
        public void HandleJoinLogin(User User, string data)
        {
            try
            {
                string user = Helper.StringBet(data, "<nick><![CDATA[", "]]").ToString();
                string pass = Helper.StringBet(data, "<pword><![CDATA[", "]]").ToString();

                int tempId = 0;

                MySqlDataReader reader = MySQL.GetData("SELECT * FROM users WHERE username='" + Helper.MySqlEscape(user) + "'");
                while (reader.Read())
                {
                    tempId = Convert.ToInt32(reader["id"]);
                }
                reader.Close();
                if (MySQL.CheckUser(user))
                {
                    User.Setup(user, tempId);

                    if (pass == Helper.SwapMD5(Login.LoginKey + Login.rndk) + Login.LoginKey)
                    {
                        this.Send(User, "%xt%l%-1%");
                    }
                    else
                    {
                        this.Send(User, "%xt%e%-1%10008%");
                    }
                }
                else
                {
                    this.Send(User, "%xt%e%-1%10008%");
                }
            }
            catch { }
        }
        public void HandleJoin(User User, string data)
        {
            string[] split = data.Split('%');
            string cmd = split[3];
            switch (cmd)
            {
                case "j#js":
                    this.Send(User, "%xt%js%-1%0%1%" + User.IsMod + "%0%");
                    break;
                case "j#jr":
                    int room = Convert.ToInt32(split[5]);
                    int x = Convert.ToInt32(split[6]);
                    int y = Convert.ToInt32(split[7]);
                    User.JoinRoom(room, x, y);
                    break;
            }
        }
        public void HandleInventory(User User, string data)
        {
            string[] split = data.Split('%');
            string cmd = split[3];
            switch (cmd)
            {
                case "i#gi":
                    String items = String.Join("%", User.Items);

                    this.Send(User, "%xt%gi%-1%" + items + "%");
                    this.Send(User, "%xt%gps%-1%" + User.Id + "%9|10|11|14|20|183%");
                    this.Send(User, "%xt%lp%-1%" + User.GenerateUserString(User.Username) + "%" + User.Coins + "%0%1440%" + Helper.LongRandom(1200000000000, 1500000000000, new Random()) + "%999%4%233%%7%");
                    this.Send(User, "%xt%glr%-1%3239%");

                    User.JoinRoom(801, 150, 150);
                    break;
            }
        }
        public void HandleUser(User User, string data)
        {
            string[] split = data.Split('%');
            string cmd = split[3];
            switch (cmd)
            {
                case "u#gp":
                    this.Send(User, "%xt%gp%" + User.GenerateUserString(User.Username) + "%");
                    break;
                case "u#h":
                    User.Send("%xt%h%-1%");
                    break;
                case "u#sp":
                    this.HandleMove(User, data);
                    break;
                case "u#sf":
                    User.UpdateFrame(split[5]);
                    break;
                case "u#sb":
                    User.SendRoom("%xt%sb%-1%" + User.Id + "%" + split[5] + "%" + split[6] + "%");
                    break;
                case "s#upc":
                    User.UpdateColor(split[5]);
                    break;
                case "s#uph":
                    User.UpdateHead(split[5]);
                    break;
                case "s#upf":
                    User.UpdateFace(split[5]);
                    break;
                case "s#upn":
                    User.UpdateNeck(split[5]);
                    break;
                case "s#upb":
                    User.UpdateBody(split[5]);
                    break;
                case "s#upa":
                    User.UpdateHand(split[5]);
                    break;
                case "s#upe":
                    User.UpdateFeet(split[5]);
                    break;
                case "s#upp":
                    User.UpdatePhoto(split[5]);
                    break;
                case "s#upl":
                    User.UpdatePin(split[5]);
                    break;
            }
        }
        public void HandleMove(User User, string data)
        {
            string[] split = data.Split('%');
            User.X = Convert.ToInt32(split[5]);
            User.Y = Convert.ToInt32(split[6]);
            User.SendRoom("%xt%sp%" + User.Room + "%" + User.Id + "%" + User.x + "%" + User.y + "%");
        }
        public void HandleAddItem(User User, string data)
        {
            string[] split = data.Split('%');
            String item = split[5];
            User.AddItem(item);
        }
        public void HandleMessage(User User, string data)
        {
            string[] split = data.Split('%');
            string cmd = split[3];
            switch (cmd)
            {
                case "m#sm":
                    String id = split[5];
                    String message = split[6];
                    if (message.StartsWith("/"))
                    {
                        HandleCommand(User, message);
                    }
                    else
                    {
                        User.SendRoom("%xt%sm%-1%" + id + "%" + message + "%");
                    }
                    break;
            }
        }
        public void HandleCommand(User User, String command)
        {
            string temp = command.Substring(1);
            string[] cmd = temp.Split(' ');
            switch (cmd[0])
            {
                case "ping":
                    User.Send("%xt%sm%-1%4433%Pong!%");
                    break;
                case "ai":
                    string[] item = command.Split(' ');
                    try
                    {
                        User.AddItem(item[1]);
                    }
                    catch { }
                    break;
                case "redeem":
                    string[] split = command.Split(' ');
                    foreach (String l in Server.Redeem.RedeemItems)
                    {
                        String code = Helper.StringBet(l, "code=", "&");
                        String type = Helper.StringBet(l, "&type=", "&");
                        String reward = Helper.StringBet(l, "&reward=", ";");

                        if (code == split[1])
                        {
                            if (type == "coins")
                            {
                                // add coins
                            }
                            else if (type == "item")
                            {
                                User.AddItem(reward);
                            }
                        }
                    }
                    break;
            }
        }
        public void Send(User user, string data)
        {
            if (Config.ServerDebug == "true")
            {
                Logger.Write("Sending: " + data);
            }

            NetworkStream ClientStream = user.connection;
            ASCIIEncoding ASCIIEncoding = new ASCIIEncoding();
            byte[] buffer = ASCIIEncoding.GetBytes(data + "\0");
            ClientStream.Write(buffer, 0, buffer.Length);
            ClientStream.Flush();
        }
    }
}


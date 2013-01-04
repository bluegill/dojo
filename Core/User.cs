using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

namespace Dojo.Core
{
    class User
    {
        /* Dojo | Club Penguin Emulator
         * Written by nich (http://pure.so/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        private int id = 0;
        public String username;
        private String loginkey = "";
        public List<int> items = new List<int> { };
        public int coins;
        public int frame = 0;
        private String isMod = "false";

        public int room;

        public int x = 0;
        public int y = 0;

        private String CurrentColor;
        private String CurrentHead;
        private String CurrentFace;
        private String CurrentNeck;
        private String CurrentBody;
        private String CurrentHand;
        private String CurrentFeet;
        private String CurrentPin;
        private String CurrentPhoto;

        public Game Game;
        public NetworkStream connection;
        public MySQL MySQL = Server.MySQL;

        public User(NetworkStream connection, Game Game)
        {
            this.Game = Game;
            this.connection = connection;
        }
        public void Setup(String user, int id)
        {
            this.Id = id;
            this.Username = user;

            try
            {
                MySqlDataReader reader = MySQL.GetData("SELECT * FROM users WHERE username='" + Helper.MySqlEscape(user) + "'");

                while (reader.Read())
                {

                    this.IsMod = reader["isModerator"].ToString();
                    this.Coins = Convert.ToInt32(reader["coins"]);
                    String tempItems = reader["items"].ToString();
                    string[] splitItems = reader["items"].ToString().Split(';');
                    foreach (String item in splitItems)
                    {
                        this.Items.Add(Convert.ToInt32(item));
                    }
                }

                reader.Close();
            }
            catch { }
        }
        public string GenerateUserString(String user)
        {
            try
            {
                MySqlDataReader reader = MySQL.GetData("SELECT * FROM users WHERE username='" + Helper.MySqlEscape(user) + "'");
                while (reader.Read())
                {
                    this.CurrentColor = reader["currentColor"].ToString();
                    this.CurrentHead = reader["currentHead"].ToString();
                    this.CurrentFace = reader["currentFace"].ToString();
                    this.CurrentNeck = reader["currentNeck"].ToString();
                    this.CurrentBody = reader["currentBody"].ToString();
                    this.CurrentHand = reader["currentHand"].ToString();
                    this.CurrentFeet = reader["currentFeet"].ToString();
                    this.CurrentPin = reader["currentPin"].ToString();
                    this.CurrentPhoto = reader["currentPhoto"].ToString();
                }
                reader.Close();

                String UserString = this.Id + "|" + user + "|1|" + this.CurrentColor +
                "|" + this.CurrentHead + "|" + this.CurrentFace + "|" + this.CurrentNeck + "|" + this.CurrentBody +
                "|" + this.CurrentHand + "|" + this.CurrentFeet + "|" + this.CurrentPin + "|" + this.CurrentPhoto + "|" + this.X + "|" + this.Y + "|" + this.Frame + "|1|1";

                return UserString;
            }
            catch { return null; }
        }
        public void SendRoom(string data)
        {
            foreach (User user in this.Game.Users)
            {
                if (user.room == this.room)
                {
                    if (user != this)
                    {
                        user.Send(data);
                    }
                }
            }
        }
        public void SendGlobal(String data)
        {
            foreach (User user in this.Game.Users)
            {
                if (user != this)
                {
                    user.Send(data);
                }
            }
        }
        public void AddItem(String item)
        {
            try
            {
                int check;
                bool isNum = int.TryParse(item, out check);
                MySqlDataReader reader = MySQL.GetData("SELECT * FROM users WHERE username='" + Helper.MySqlEscape(this.Username) + "'");
                String currItems = "";
                while (reader.Read())
                {
                    currItems = reader["items"].ToString();
                }
                reader.Close();

                if (!isNum)
                {
                    this.Send("%xt%e%-1%402%");
                }
                else if (!this.Items.Contains(Convert.ToInt32(item)))
                {
                    MySQL.Execute("UPDATE users SET items='" + currItems + ";" + item + "' WHERE username='" + this.Username + "';");
                    this.Items.Add(Convert.ToInt32(item));
                    this.Send("%xt%ai%-1%" + item + "%1000000%");
                }
                else
                {
                    this.Send("%xt%e%-1%400%");
                }
            }
            catch { }
        }
        public void JoinRoom(int room, int UserX, int UserY, bool isIgloo = false)
        {
            this.SendGlobal("%xt%rp%-1%" + this.Id + "%");

            this.Room = room;
            this.X = UserX;
            this.Y = UserY;

            String BotString = "4433|" + Config.BotName + " [BOT]|1|7|0|410|0|0|0|0|0|0|0|0|" + Helper.IntRandom(0, 50) + "|1|1";
            String FinalString = BotString;

            FinalString += "%";
            FinalString += this.GenerateUserString(this.Username);

            String packet = "%xt%jr%-1%" + room + "%" + FinalString + "%";

            foreach (User user in this.Game.Users)
            {
                if (user.Username != this.Username)
                {
                    if (user.room == this.room)
                    {
                        packet += user.GenerateUserString(user.Username) + "%";
                    }
                }
            }
            
            this.Send(packet);
            this.SendRoom("%xt%ap%-1%" + this.GenerateUserString(this.Username) + "%");
        }
        public void Send(string data)
        {
            if (Config.ServerDebug == "true")
            {
                Logger.Write("Sending: " + data);
            }

            NetworkStream ClientStream = this.connection;
            ASCIIEncoding ASCIIEncoding = new ASCIIEncoding();
            byte[] buffer = ASCIIEncoding.GetBytes(data + "\0");
            ClientStream.Write(buffer, 0, buffer.Length);
            ClientStream.Flush();
        }
        public void UpdateFrame(String frame)
        {
            this.Frame = Convert.ToInt32(frame);
            this.SendRoom("%xt%sf%-1%" + this.Id + "%" + frame + "%");
        }
        public void UpdateColor(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentColor='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentColor = item;
                this.Send("%xt%upc%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%upc%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public void UpdateHead(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentHead='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentHead = item;
                this.Send("%xt%uph%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%uph%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public void UpdateFace(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentFace='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentFace = item;
                this.Send("%xt%upf%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%upf%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public void UpdateNeck(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentNeck='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentNeck = item;
                this.Send("%xt%upn%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%upn%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public void UpdateBody(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentBody='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentBody = item;
                this.Send("%xt%upb%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%upb%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public void UpdateHand(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentHand='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentHand = item;
                this.Send("%xt%upa%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%upa%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public void UpdateFeet(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentFeet='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentFeet = item;
                this.Send("%xt%upe%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%upe%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public void UpdatePin(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentPin='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentPin = item;
                this.Send("%xt%upl%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%upl%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public void UpdatePhoto(String item)
        {
            try
            {
                MySQL.Execute("UPDATE users SET currentPhoto='" + item + "' WHERE username='" + this.Username + "';");
                this.CurrentPhoto = item;
                this.Send("%xt%upp%-1%" + this.Id + "%" + item + "%");
                this.SendRoom("%xt%upp%-1%" + this.Id + "%" + item + "%");
            }
            catch { }
        }
        public String Username
        {
            get { return this.username; }
            set { this.username = value; }
        }
        public int Frame
        {
            get { return this.frame; }
            set { this.frame = value; }
        }
        public String LoginKey
        {
            get { return this.loginkey; }
            set { this.loginkey = value; }
        }
        public String IsMod
        {
            get { return this.isMod; }
            set { this.isMod = value; }
        }
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }
        public int Coins
        {
            get { return this.coins; }
            set { this.coins = value; }
        }
        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }
        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }
        public List<int> Items
        {
            get { return this.items; }
            set { this.items = value; }
        }
        public int Room
        {
            get { return this.room; }
            set { this.room = value; }
        }
        public NetworkStream Connection
        {
            get { return this.connection; }
            set { this.connection = value; }
        }
    }
}

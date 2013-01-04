using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Dojo.Core
{
    class MySQL
    {
        /* Dojo | Club Penguin Emulator
         * Written by nich (http://pure.so/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public MySqlConnection connection;

        public MySQL()
        {
            Logger.Write("Connecting to MySQL...");

            String ConnString = "Server=" + Config.MySqlHost + ";Database=" + Config.MySqlDatabase + ";Uid=" + Config.MySqlUser + ";Pwd=" + Config.MySqlPass + ";";

            this.connection = new MySqlConnection(ConnString);

            try
            {
                this.connection.Open();
            }
            catch (MySqlException ex)
            {
                Logger.WriteError(ex.Message);
            }
        }
        public MySqlDataReader GetData(string cmd)
        {
            MySqlCommand command = this.connection.CreateCommand();
            command.CommandText = cmd;
            MySqlDataReader reader = command.ExecuteReader();
            return reader;
        }
        public void Execute(String cmd)
        {
            MySqlCommand command = this.connection.CreateCommand();
            command.CommandText = cmd;
            command.ExecuteNonQuery();
        }
        public bool CheckUser(String user)
        {
            MySqlCommand command = this.connection.CreateCommand();
            command.CommandText = "SELECT * FROM users WHERE username='" + Helper.MySqlEscape(user) + "'";
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                return true;
            }
            else
            {
                reader.Close();
                return false;
            }
        }
    }
}

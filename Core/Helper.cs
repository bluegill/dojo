using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Dojo.Core
{
    class Helper
    {
        /* Dojo | Club Penguin Emulator
         * Written by Seether (http://tiber.me/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public static int IntRandom(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
        public static long LongRandom(long min, long max, Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }
        public static string GenerateLoginKey(int rndk)
        {
            String MD5 = GenerateMD5(rndk.ToString());
            return StringRev(MD5);
        }
        public static string GenerateMD5(string text)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(text);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }

            String TS = s.ToString();
            return TS;
        }
        
        public static string StringRev(string text)
        {
            if (text == null) return null;
            char[] array = text.ToCharArray();
            Array.Reverse(array);
            return new String(array);
        }
        public static string StringBet(string Subject, string Start, string End)
        {
            return Regex.Match(Subject, Regex.Replace(Start, @"[][{}()*+?.\\^$|]", @"\$0") + @"\s*(((?!" + Regex.Replace(Start, @"[][{}()*+?.\\^$|]", @"\$0") + @"|" + Regex.Replace(End, @"[][{}()*+?.\\^$|]", @"\$0") + @").)+)\s*" + Regex.Replace(End, @"[][{}()*+?.\\^$|]", @"\$0"), RegexOptions.IgnoreCase).Value.Replace(Start, "").Replace(End, "");
        }
        public static string SwapMD5(String password)
        {
            String pwd = GenerateMD5(password);
            String Final = pwd.Substring(16, 16) + pwd.Substring(0, 16);
            return Final;
        }
        public static string HashPass(String password, int rndk)
        {
            String x = SwapMD5(password);
            String y = x.ToUpper();
            y += rndk;
            // FOR AS3: y += "a1ebe00441f5aecb185d0ec178ca2305Y(02.>'H}t\":E1_root";
            y += "Y(02.>'H}t\":E1";
            y = SwapMD5(y);
            return(y);
        }
        public static string MySqlEscape(string str)
        {
            if (str == null)
            {
                return null;
            }
            return Regex.Replace(str, @"[\r\n\x00\x1a\\'""]", @"\$0");
        }
        public static string GetFileContents(string fileName)
        {
            string sContents = string.Empty;

            if (fileName.ToLower().IndexOf("http:") > -1)
            {
                try
                {
                    System.Net.WebClient wc = new System.Net.WebClient();
                    byte[] response = wc.DownloadData(fileName);
                    sContents = System.Text.Encoding.ASCII.GetString(response);
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex.Message);
                }
            }
            else
            {
                try
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
                    sContents = sr.ReadToEnd();
                    sr.Close();
                }
                catch
                {
                    Logger.WriteError("Could not find file!");
                }
            }
            return sContents;
        }
    }
}

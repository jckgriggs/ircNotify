using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace ircNotify
{
    class Program
    {
        public static string SERVER;
        public static int PORT = 6667;
        public static string CHANNEL;
        static Random rand = new Random();
        static int randomNumber = rand.Next(1, 99999);
        public static string NICK = "ircNotify" + randomNumber;
        private static string USER = "USER ircNotify 0 * :ircNotify";
        public static string userName;
        static StreamWriter writer;
        static string lastUser;


        static void Main(string[] args)
        {
        infoInitial:
            Console.WriteLine("Enter server: ");
            SERVER = Console.ReadLine();
        infoPort:
            Console.WriteLine("Enter port: ");
            try
            {
                PORT = Convert.ToInt32(Console.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine("That's not a number...");
                goto infoPort;
            }
            Console.WriteLine("Enter channel: ");
            CHANNEL = Console.ReadLine();
            Console.WriteLine("Enter your current username: ");
            userName = Console.ReadLine();
          infoConfirm:
            Console.WriteLine("Is this information correct? (Y/N):\nServer: " + SERVER + "\nPort: " + PORT + "\nChannel: " + CHANNEL + "\nUser: " + userName);
            string infoCorrect = Console.ReadLine().ToLower();
            if (infoCorrect == "y")
            {
                Console.WriteLine("Connecting...");
                ircNotify();
            }
            else if (infoCorrect == "n")
            {
                Console.WriteLine("Please hit enter to re-enter your information.");
                Console.ReadLine();
                Console.Clear();
                goto infoInitial;
            }
            else
            {
                Console.WriteLine("That's not an option, please hit enter to re-review your information.");
                Console.ReadLine();
                Console.Clear();
                goto infoConfirm;
            }
        }
        static void playPos()
        {
            var playPos = new System.Media.SoundPlayer();
            playPos.Stream = Properties.Resources.notifyPos;
            playPos.Play();
        }
        static void playNeg()
        {
            var playNeg = new System.Media.SoundPlayer();
            playNeg.Stream = Properties.Resources.notifyNeg;
            playNeg.Play();
        }
        static void playMsg()
        {
            var playMsg = new System.Media.SoundPlayer();
            playMsg.Stream = Properties.Resources.notifyMsg;
            playMsg.Play();
        }
        static void ircNotify()
        {
            NetworkStream stream;
            TcpClient irc;
            string inputLine;
            StreamReader reader;

            try
            {

                irc = new TcpClient(SERVER, PORT);
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                writer.WriteLine("NICK " + NICK);
                writer.Flush();
                writer.WriteLine(USER);
                writer.Flush();
                while (true)
                {
                Start:
                    while ((inputLine = reader.ReadLine()) != null)
                    {
                        //Console.WriteLine("<-" + inputLine);
                        string[] splitInput = inputLine.Split(new Char[] { ' ' });
                        string output = inputLine.Substring(inputLine.IndexOf("&") + 1);
                        //string[] str = output.Split(' ');

                        if (splitInput[0] == "PING")
                        {
                            string PongReply = splitInput[1];
                            writer.WriteLine("PONG " + PongReply);
                            writer.Flush();
                            continue;
                        }

                        switch (splitInput[1])
                        {
                            case "001":
                                string JoinString = "JOIN " + CHANNEL;
                                writer.WriteLine(JoinString);
                                writer.Flush();
                                Console.WriteLine("Connected");
                                Thread.Sleep(3000);
                                Console.Clear();
                                break;
                            default:
                                break;
                        }
                        if (inputLine != null)
                        {
                            if (inputLine.Contains("JOIN"))
                            {
                                string userAffected = inputLine.Split(new char[] { ':', '!' })[1];
                                if (userAffected == NICK || userAffected == userName)
                                {
                                    goto Start;
                                }
                                else
                                {
                                    //Begin Console Writing
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(userAffected + " has joined!");
                                    Console.ResetColor();
                                    //End Console Writing
                                    playPos();
                                }
                            }
                            else if (inputLine.Contains("PART"))
                            {
                                string userAffected = inputLine.Split(new char[] { ':', '!' })[1];
                                if (userAffected == userName)
                                {
                                    goto Start;
                                }
                                else
                                {
                                    //Begin Console Writing
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(userAffected + " has left :<");
                                    Console.ResetColor();
                                    //End Console Writing
                                    playNeg();
                                }
                            }
                            else if (inputLine.Contains("PRIVMSG"))
                            {
                                string userAffected = inputLine.Split(new char[] { ':', '!' })[1];
                                if (userAffected == NICK || userAffected == userName || userAffected == lastUser)
                                {
                                    goto Start;
                                }
                                else if(output.Contains(userName))
                                {
                                    //Begin Console Writing
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine(userAffected + " has mentioned you!");
                                    Console.ResetColor();
                                    //End Console Writing
                                    playMsg();
                                    lastUser = userAffected;
                                }
                            }
                        }
                    }
                    writer.Close();
                    reader.Close();
                    irc.Close();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Thread.Sleep(5000);
                string[] argv = { };

            }
        }
    }
}

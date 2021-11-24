using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using TLFunctionalityLib;
using System.Threading;

namespace MessageScheduler
{
    public class Program
    {

        private static string _exePath = Assembly.GetExecutingAssembly().Location;

        public const string serviceName = "TLMessageScheduler";

        public string MessagesPath;

        private static TelegramMessage msgToSend;

        public static void Main(string[] args)
        {
            [DllImport("kernel32.dll")]
            static extern IntPtr GetConsoleWindow();

            [DllImport("user32.dll")]
            static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            const int SW_HIDE = 0;

            var handle = GetConsoleWindow();

            // Hide
            ShowWindow(handle, SW_HIDE);

            Console.WriteLine("starting main..");
            //Console.ReadLine();

            MainAsync(args).GetAwaiter().GetResult();
            Console.WriteLine("main finished");
            //Console.ReadLine();
        }
    
        public static async Task MainAsync(string[] args)
        {
            if (args.Length < 1)
                return;

         
            string[] arguments = args[args.Length-1].Split(",");
            

            string path = arguments[0];
            if (path.Contains("*"))
            {
                path = path.Replace("*", " ");
            }
            
            Console.WriteLine($"path: {path}");
            //Console.ReadLine();
            //string sessionFilePath = argsuments[1];
            int ApiAppID = int.Parse(arguments[1]);
            string ApiHASH = arguments[2];
            //string sessName = arguments[3];
            //string sessPath = arguments[4];

            //SessionStore sessionStore = new SessionStore() { Name = sessName, SeesionPath = sessPath };

            Console.WriteLine($"path: {path}");
            Console.WriteLine($"App id: {ApiAppID}");
            Console.WriteLine($"App hash: {ApiHASH}");

            
            //Console.WriteLine($"sess path: {sessionStore.SeesionPath}");

            StreamReader sr = new StreamReader(path);
           
            XmlSerializer serializer = new XmlSerializer(typeof(TelegramMessage));
            object msg =  serializer.Deserialize(sr);

            if (msg is not TelegramMessage)
            {
                // debug
                Console.WriteLine("Fail deserialize");
               // Console.ReadLine();
                return;
            }

           
            sr.Close();
            File.Delete(path);
            msgToSend = (TelegramMessage)msg;

            List<string> users = msgToSend.destChatId;

            Console.WriteLine($"users count: {users.Count} : ");


            Console.WriteLine(msgToSend.ToString());
            //Console.ReadLine();


            // todo initialize user details
            TelegramUserDetails details = TelegramUserDetails.Instance;
            details.ApiHASH = ApiHASH;
            details.ApiAppID = ApiAppID;

            TelegramClientSingelton clientSingelton = new TelegramClientSingelton();

            bool result = false;
            switch (msgToSend.properties.msgType)
            {
                case MsgType.users:
                    result = await clientSingelton.BroadcastMessage(msgToSend.destChatId, msgToSend);
                    break;
                case MsgType.channel:
                    result = await clientSingelton.SendMessageToGroupChannel(msgToSend.destChatId[0] ,false ,msgToSend);
                    break;
                case MsgType.group:
                    result = await clientSingelton.SendMessageToGroupChannel(msgToSend.destChatId[0] ,true ,msgToSend);
                    break;
                case MsgType.excel:
                    foreach (string dest in msgToSend.destChatId)
                    {
                        if (isDigitsOnly(dest))
                        {
                            try
                            {
                                result = await clientSingelton.SendMessageToUser(dest, msgToSend);
                            }
                            catch (TLSharp.Core.Network.Exceptions.FloodException e)
                            {
                                Thread.Sleep((int)e.TimeToWait.TotalMilliseconds + 10);
                                result = await clientSingelton.SendMessageToUser(dest, msgToSend);
                            }
                        }
                        else
                        {
                            try
                            {
                                result = await clientSingelton.SendToTelegramUser(dest, msgToSend);
                            }
                            catch (TLSharp.Core.Network.Exceptions.FloodException e)
                            {
                                Thread.Sleep((int)e.TimeToWait.TotalMilliseconds + 10);
                                result = await clientSingelton.SendToTelegramUser(dest, msgToSend);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            if(result)
            {
                Console.WriteLine("sucess");
            }
            else
            {
                Console.WriteLine("failure");
            }
            //Console.ReadLine();


        }

        public static bool isDigitsOnly(string inStr)
        {
            foreach (char c in inStr.Substring(1, inStr.Length - 1))
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }
            return true;

        }

        public static bool InstallService()
        {


            PowerShell ps = PowerShell.Create();
            ps.AddCommand($"New-Service");
            ps.AddParameter("Name", serviceName);
            ps.AddParameter("BinaryPathName", _exePath);
            try
            {
                ps.Invoke();
            }
            catch
            {
                return false;
            }
            return ps.HadErrors;
        }

        private static void fixExePath()
        {
            if (_exePath.Contains(".dll"))
            {
                int idx = _exePath.IndexOf(".dll");
                _exePath = _exePath.Substring(0, idx) + ".exe";
            }
        }

        public static bool UninstallService()
        {
            

            PowerShell ps = PowerShell.Create();
            ps.AddCommand("Remove-Service");
            ps.AddParameter("Name", serviceName);
            try
            {
                ps.Invoke();
            }
            catch
            {
                return false;
            }
            return true;
        } 
    }
}

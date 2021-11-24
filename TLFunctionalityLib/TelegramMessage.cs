using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using System.Windows;

namespace TLFunctionalityLib
{
    public enum MediaType
    {
        Non,
        document,
        image
    };
    public enum MsgType
    {
        channel,
        group,
        users,
        excel
    }

    public class MessageProperties
    {
        public List<string> dest { get; set; }
        public MsgType msgType { get; set; }
    }
    public class TelegramMessage
    {
        private string _mediaPath = "";
        public string Name { get; set; } //  text
        public string Message { get; set; } // text 

        // path to media to be attached
        public string MediaPath {
            get { return _mediaPath; }
            set
            {
                _mediaPath = value;
            }
        }
        public string fileName { get; set; } = ""; // file name

        public string MimeType { get; set; }
        public MediaType mediaType { get; set; } // file media type
        //public Textbox Alignment { get; set; }

        [XmlIgnore]
        public DateTime date { get; set; } // date to send the message
        public bool isScheduled { get; set; } // is message scheduled flag variable

        //path to file where all messages are saved
        public static readonly string PATH_TO_MESSAGES = Path.Combine(Directory.GetCurrentDirectory(), "Messages");

        public List<string> destChatId { get; set; }

        public MessageProperties properties { get; set; }


        [XmlIgnore]
        public static TLDialogs clientDialogs;
        [XmlIgnore]
        public static TLUser clientUsers;

        public TelegramMessage()
        {
            Name = default;
            Message = default;
            MediaPath = default;
       //     Alignment = null;
            isScheduled = false;
            mediaType = MediaType.Non;
            destChatId = new List<string>();
        }

        public bool DeleteMessageFromStorage()
        {
            try
            {
                File.Delete($"{PATH_TO_MESSAGES}\\{Name}.xaml");
                return true;
            }
            catch { return false; }
        }

        public static bool DeleteMessageFromStorage(string name)
        {
            try
            {
                File.Delete($"{PATH_TO_MESSAGES}\\{name}.xaml");
                return true;
            }
            catch { return false; }
        }

        

        //return list of all messages
        public static List<TelegramMessage> LoadFromFile()
        {
            List<TelegramMessage> list = new List<TelegramMessage>();
            if (!Directory.Exists(PATH_TO_MESSAGES))
            {
                Directory.CreateDirectory(PATH_TO_MESSAGES);
                return list;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(TelegramMessage));

            string[] files = Directory.GetFiles(PATH_TO_MESSAGES);
            if (files.Length == 0)
                return list;

            foreach (var name in files)
            {
                using (StreamReader sr = new StreamReader(name))
                {
                    TelegramMessage message = (TelegramMessage)serializer.Deserialize(sr);
                    list.Add(message);
                }
            }
            return list;
        }

        //check if there is a message under a given name
        public static bool MessageNameExists(string name)
        {
            return Directory.GetFiles(PATH_TO_MESSAGES).Contains($"{PATH_TO_MESSAGES}\\{name}.xaml");
        }

        
        private XmlSerializer serializer { get; set; } = new XmlSerializer(typeof(TelegramMessage)); //serializer

        public void Save()
        {
            date = default;
            isScheduled = false;
            string path = $"{PATH_TO_MESSAGES}\\{Name}.xaml";
            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, this);
            }
        }

        public override string ToString()
        {
            return "Messge" + Message + "\nDate Time: " + date.ToString();
        }
        
    }
}

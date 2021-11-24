using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using TeleSharp.TL;
using TeleSharp.TL.Messages;

namespace MessageQueueService.DataClasses
{ 
    public enum MediaType
    {
        Non,
        document,
        image
    };
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
        public TextAlignment Alignment { get; set; }

        public DateTime date { get; set; } // date to send the message
        public bool isScheduled { get; set; } // is message scheduled flag variable

        //path to file where all messages are saved
        public const string PATH_TO_MESSAGES = "Messages";


        [XmlIgnore]
        public static TLDialogs clientDialogs;
        [XmlIgnore]
        public static TLUser clientUsers;

        public TelegramMessage()
        {
            Name = default;
            Message = default;
            MediaPath = default;
            Alignment = default;
            isScheduled = false;
            mediaType = MediaType.Non;
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
            foreach (var name in Directory.GetFiles(PATH_TO_MESSAGES))
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
            string path = $"{PATH_TO_MESSAGES}\\{Name}.xaml";
            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, this);
            }
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TLFunctionalityLib
{
    public class TelegramUserDetails
    {
        private TelegramUserDetails() { }

        public string PhoneNumber { get; set; }
        public int ApiAppID { get; set; }
        public string ApiHASH { get; set; }

        [XmlIgnore]
        private static TelegramUserDetails instance = null;

        [XmlIgnore]
        public static TelegramUserDetails Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TelegramUserDetails();
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }
        public void Create(string PhoneNumber, int ApiAppID, string ApiHASH)
        {
            this.PhoneNumber = PhoneNumber;
            this.ApiAppID = ApiAppID;
            this.ApiHASH = ApiHASH;
        }
    }
}

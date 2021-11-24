using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MessageQueueService.DataClasses
{
    public class TelegramUserDetails
    {

        private TelegramUserDetails() { }

        public string PhoneNumber { get; set; } = "+972528383211";
        public int ApiAppID { get; set; } = 7794876;
        public string ApiHASH { get; set; } = "91fee9930a3de3417bb1e6f8830aa5fd";

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
        }
    }
}

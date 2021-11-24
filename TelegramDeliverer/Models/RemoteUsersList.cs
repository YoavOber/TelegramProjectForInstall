using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramDeliverer.Models
{
    public class RemoteUsersList : IEnumerable
    {
        public ObservableCollection<string> List { get; set; } = new ObservableCollection<string>();

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)List).GetEnumerator();
        }
    }
}

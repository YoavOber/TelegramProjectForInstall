using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TelegramDeliverer.Models;

namespace TelegramDeliverer.Pages
{
    /// <summary>
    /// Interaction logic for ListViewPage.xaml
    /// </summary>
    public partial class ListViewPage : Window
    {
        public RemoteUsersList remoteUsersList { get; set; }
        private string selectedChat;
        public ListViewPage(RemoteUsersList list)
        {
            InitializeComponent();
            usersListView.ItemsSource = list;
            usersListView.MouseDoubleClick += OnListViewItemClick;

            remoteUsersList = list;
        }

        private void OnListViewItemClick(object sender, EventArgs e)
        {
            if (sender == null)
                return;
           
            ListBox listBox = (ListBox)sender;
            if (listBox.SelectedItem == null)
            {
                MessageBox.Show("לא נבחר נמען.");
                Close();
                return;
            }
            selectedChat = listBox.SelectedItem.ToString();
            MessageBox.Show("ההודעה תופץ למשתמשי הקבוצה : "+ selectedChat);
            Close();
        }

        public string ShowDialog()
        {
            base.ShowDialog();
            return selectedChat;
        }
    }
}

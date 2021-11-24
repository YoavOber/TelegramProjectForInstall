using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using TelegramDeliverer.Models;
using TelegramDeliverer.Pages;
using TLFunctionalityLib;

namespace TelegramDeliverer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainPage Main { get; set; }
        public DeliveryPage DeliveryPage { get; set; }
        public MainWindow()
        {
            Main = new MainPage();

            // load details from file
            try
            {
                using (StreamReader sr = new StreamReader("TelegramUserDetails.xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TelegramUserDetails));

                    TelegramUserDetails details = (TelegramUserDetails)serializer.Deserialize(sr);
                    TelegramUserDetails.Instance = details;
                    TelegramClientSingelton.Instance.userDetails = TelegramUserDetails.Instance;
                }
            }
            catch (Exception e)
            {
                if (e is IOException || e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    if(MessageBox.Show("קובץ קונפיגורציה לא נמצא") > 0)
                        Environment.Exit(0);
                }
            }

            Main.OnChangeScreen += ChangeToDeliveryScreen;
            InitializeComponent();
            Content = Main;
        }

        private void ChangeToDeliveryScreen(TelegramMessage msg)
        {
            try
            {
                DeliveryPage = new DeliveryPage(msg);
                DeliveryPage.OnChangeScreen += ChangeToMainScreen;
                // DeliveryPage.onchangescreen += TODO
                Content = DeliveryPage;
            }
            catch(Exception e)
            {
                MessageBox.Show("תקלה - " + e.Message);
            }
        }

        private void ChangeToMainScreen()
        {
            Content = Main;
        }
    }
}
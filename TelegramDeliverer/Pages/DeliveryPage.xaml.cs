using Microsoft.Win32.TaskScheduler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
using TelegramDeliverer.ViewModels;
using TLFunctionalityLib;

namespace TelegramDeliverer.Pages
{
    /// <summary>
    /// Interaction logic for DeliveryPage.xaml
    /// </summary>
    public partial class DeliveryPage : Page
    {
        public delegate void ChangeScreenEvent();
        public ChangeScreenEvent OnChangeScreen { get; set; }

        public DeliveryViewModel VM { get;set; } // view model
        public DeliveryPage(TelegramMessage msg)
        {
            InitializeComponent();
            VM = new DeliveryViewModel(msg);
            DataContext = VM;

            DatePicker.SelectedDate = DateTime.Now;
            TimePicker.SelectedTime = DateTime.Now;

        }

        private void EscBtnClick(object sender, RoutedEventArgs e)
        {
            OnChangeScreen.Invoke();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VM.CurrentMessage.isScheduled = true;
            DatePicker.Visibility = Visibility.Visible;
            TimePicker.Visibility = Visibility.Visible;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            VM.CurrentMessage.isScheduled = false;
            VM.SelectedDate = default; // ערבי רצח  
            DatePicker.Visibility = Visibility.Hidden;
            TimePicker.Visibility = Visibility.Hidden;
        }

        private void Text_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Text_Box.Text.Length > 0)
            {
                char first = Text_Box.Text[0];
                Text_Box.TextAlignment = (first >= 'א' && first <= 'ת') ? TextAlignment.Right : TextAlignment.Left;
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePicker.SelectedDate != null && VM != null)
            {
                VM.CurrentMessage.date = (DateTime)DatePicker.SelectedDate;

                if (VM.CurrentMessage.properties == null)
                    VM.CurrentMessage.properties = VM.SelectedMsgProperties;
            }
        }

        private void TimePicker_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            if (TimePicker.SelectedTime != null && VM != null)
            {
                var selectedHour= TimePicker.SelectedTime.Value;
                var selectedDate = DatePicker.SelectedDate.Value;
                var newDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, selectedHour.Hour, selectedHour.Minute, selectedHour.Second);
                VM.CurrentMessage.date = newDate;

                if (VM.CurrentMessage.properties == null)
                    VM.CurrentMessage.properties = VM.SelectedMsgProperties;
            }

        }
    }
}

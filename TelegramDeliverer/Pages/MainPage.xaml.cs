using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using TelegramDeliverer.Models;
using MimeMapping;
using MimeKit;
using MessageScheduler;
using TLFunctionalityLib;
using System.ServiceProcess;
using System.Collections.Generic;

namespace TelegramDeliverer.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private const string defaultText = "הכנס טקסט כאן Enter Text Here";

        public delegate void ChangeScreenEvent(TelegramMessage message);//delegate ued by MAINWINDOW to know to switch between pages
        public ChangeScreenEvent OnChangeScreen { get; set; }


        #region media properties
        // File media path
        private string mediaPath { get; set; }
        // file mime type
        private string mimeType { get; set; }
        // file type
        private MediaType type { get; set; }
        // full file name with extension
        private string fileName { get; set; }
        #endregion
        public MainPage()
        {
            InitializeComponent();


            List<ServiceController>services = new List<ServiceController>(ServiceController.GetServices());


            if (services.Find(X => X.ServiceName == Program.serviceName) == null)
            {
                // service does not exists.
                //bool isSuccess = Program.InstallService();

            }

            mediaPath = null;

        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Text_Box.Text = Text_Box.Text == defaultText ? string.Empty : Text_Box.Text;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Text_Box.Text = Text_Box.Text == string.Empty ? defaultText : Text_Box.Text;
        }

        private void Text_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CharCounter != null)
            {
                CharCounter.Text = Text_Box.Text.Length.ToString();
            }
            if (TextBlockCounter != null)
            {
                int n = Text_Box.Text.Split("\r\n\r\n").Length - 1;
                int count = (Text_Box.Text.Length > 0) ? n + 1 : n;
                if (Text_Box.Text.EndsWith("\r\n\r\n"))
                    count--;
                TextBlockCounter.Text = count.ToString();
            }
            if (Text_Box.Text.Length > 0)
            {
                char first = Text_Box.Text[0];
                Text_Box.TextAlignment = (first >= 'א' && first <= 'ת') ? TextAlignment.Right : TextAlignment.Left;
            }
        }

        private void ClearBtnClick(object sender, RoutedEventArgs e) => Text_Box.Clear();

        private void SaveAsBtnClick(object sender, RoutedEventArgs e)
        {
            SaveBtn.Visibility = Visibility.Collapsed;
            SendBtn.Visibility = Visibility.Collapsed;
            SaveAsPopUp.Visibility = Visibility.Visible;
            if (Text_Box.Text.Length == 0)
            {
                MessageBox.Show("שימו לב! ההודעה ריקה");
            }
        }


        public bool FilePathHasInvalidChars(string path)
        {

            return !string.IsNullOrEmpty(path) && path.IndexOfAny(Path.GetInvalidPathChars()) >= 0;
        }

        private void CompleteSaveBtnClick(object sender, RoutedEventArgs e)
        {
            //do some basic input checks
            if (SaveAsTextbox.Text.Length == 0)
            {
                MessageBox.Show("לא ניתן לשמור הודעה ללא שם");
                return;
            }
            if (FilePathHasInvalidChars(SaveAsTextbox.Text))
            {
                MessageBox.Show("שם קובץ לא חוקי - אין להשתמש בתווים : " + Path.GetInvalidPathChars().ToString());
                return;
            }
            if (TelegramMessage.MessageNameExists(SaveAsTextbox.Text))
            {
                MessageBox.Show("כבר יש הודעה שמורה בשם הזה. אנא בחר שם אחר");
                return;
            }
            if (Text_Box.Text == string.Empty)
                MessageBox.Show("שים לב - ההודעה ריקה");

            //save message
            TelegramMessage msg = new TelegramMessage
            {
                Name = SaveAsTextbox.Text,
                Message = Text_Box.Text == defaultText ? string.Empty : Text_Box.Text,
                MediaPath = mediaPath,
                MimeType = mimeType,
                mediaType = type
              //  Alignment = Text_Box.TextAlignment
            };
            msg.Save();
            MessageBox.Show("נשמר בשם : " + SaveAsTextbox.Text);
            SaveAsTextbox.Clear();
            Text_Box.Clear();
            SaveAsPopUp.Visibility = Visibility.Collapsed;
            SaveBtn.Visibility = Visibility.Visible;
            SendBtn.Visibility = Visibility.Visible;
        }

        private void CancelSaveBtnClick(object sender, RoutedEventArgs e)
        {
            SaveAsTextbox.Clear();
            SaveAsPopUp.Visibility = Visibility.Collapsed;
            SaveBtn.Visibility = Visibility.Visible;
            SendBtn.Visibility = Visibility.Visible;
        }
        private void SendBtnClick(object sender, RoutedEventArgs e)
        {
            TelegramMessage message = new TelegramMessage
            {
                Message = Text_Box.Text == defaultText ? string.Empty : Text_Box.Text,
                MediaPath = mediaPath,
                Name = null,
                //Alignment = (NPOI.XWPF.UserModel.TextAlignment)Text_Box.TextAlignment,
                mediaType = type,
                MimeType = mimeType,
                fileName = this.fileName,
                
            };
            OnChangeScreen.Invoke(message);
        }

        private void AddMediaBtnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"c:\",
                RestoreDirectory = true,
                ReadOnlyChecked = true,
                ShowReadOnly = true
            };
            if ((bool)openFileDialog1.ShowDialog())
            {
                mediaPath = openFileDialog1.FileName;
                Media_Path.Text = mediaPath;

                fileName = openFileDialog1.SafeFileName; 
                mimeType = MimeTypes.GetMimeType(fileName);
                if (mimeType.ToLower().Contains("image"))
                    type = MediaType.image;
                else
                    type = MediaType.document;
            }
        }

        private void DeleteMediaBtn_Click(object sender, RoutedEventArgs e)
        {
            mediaPath = null;
            Media_Path.Text = string.Empty;
        }


        private void Text_Box_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }

}


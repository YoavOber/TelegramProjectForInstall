using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TelegramDeliverer.Models;
using TelegramDeliverer.Pages;
using TLFunctionalityLib;
using TLSharp;
using TeleSharp.TL;
using TLSharp.Core;
using Microsoft.Win32.TaskScheduler;
using System.Xml.Serialization;
using System.Threading;

namespace TelegramDeliverer.ViewModels
{
    

    
    public class DeliveryViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private TelegramMessage _currentMessage;
        public TelegramMessage CurrentMessage //currently presented message
        {
            get
            {
                return _currentMessage;
            }
            set
            {
                _currentMessage = value;
                OnPropertyChanged();
            }
        }

        public List<TelegramMessage> existingMessages { get; set; }
        public ObservableCollection<ListViewItem> SavedMessages { get; set; }
        public ICommand DeleteCmd { get; set; }//delete message (right click) command
        public ICommand SaveEditCmd { get; set; }//save edits on message command
        public ICommand FromExcelCmd { get; set; } // chose user from excel
        public ICommand FromGroupCmd { get; set; } // type group link command
        public ICommand ToPhoneNumCmd { get; set; } // chose private user command
        public ICommand ToChannelCmd { get; set; } // type telegram channel command
        public ICommand OnSendBtnCmd { get; set; } // on send button command

        private string selectedDestText { get; set; }
        public string SelectedDestinationText
        {
            get
            {
                return selectedDestText;
            }
            set
            {
                selectedDestText = value;
                OnPropertyChanged();
            }
        }//binds to selected destinaton textbox


        public ListViewItem SelectedMsg { get; set; }// = new ListViewItem();
        public string SelectedDate { get; set; }//saves selected date (if selected) 
        private TelegramClient TLSClient { get; set; }
        private TelegramClientSingelton ClientSingelton { get; set; }

        public MessageProperties SelectedMsgProperties { get; set; }

        private object lockObj = new object(); // lock used to protect sensitive queue

        public DeliveryViewModel(TelegramMessage msg)
        {
            CurrentMessage = msg;

            ClientSingelton = TelegramClientSingelton.Instance;

            ClientSingelton.userDetails = TelegramUserDetails.Instance;
            TLSClient = TelegramClientSingelton.Instance.client;

           
            if (!TLSClient.IsUserAuthorized())
            {
                doAutorization();
            }
            

            SelectedDestinationText = "לא נבחר נמען";
            SelectedMsgProperties = new MessageProperties();
            

            CurrentMessage.properties = SelectedMsgProperties;
            if (!TLSClient.IsConnected)
                doTelegramLogin();

            if (!TLSClient.IsUserAuthorized())
                Console.WriteLine("we shouldn't be here");

            // init btn's command
            DeleteCmd = new CommandHandler(param => OnDelete(), () => true);
            SaveEditCmd = new CommandHandler(param => CtrlSaveCmd(), () => true);
            FromExcelCmd = new CommandHandler(param => { OnChoseExcel(); }, () => true);
            FromGroupCmd = new CommandHandler(param => { OnChannelGroupCmd(true); }, () => true);
            ToPhoneNumCmd = new CommandHandler(param => { OnChosePrvUser(); }, () => true);
            ToChannelCmd = new CommandHandler(param => { OnChannelGroupCmd(false); }, () => true);
            OnSendBtnCmd = new CommandHandler(param => { OnSendCmd(); }, () => true);

            SavedMessages = new ObservableCollection<ListViewItem>();
            existingMessages = TelegramMessage.LoadFromFile();
            if (existingMessages.Count == 0)
            {
                SavedMessages.Add(new ListViewItem
                {
                    Content = "אין הודעות שמורות",
                    FontSize = 20,
                    FontFamily = new System.Windows.Media.FontFamily("Sans-serif"),
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right
                });
            }
            foreach (var m in existingMessages)
            {
                ListViewItem item = new ListViewItem
                {
                    Content = m.Name,
                    FontSize = 21,
                    FontFamily = new System.Windows.Media.FontFamily("Sans-serif"),
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right
                };
                item.Selected += ChangeMessageBtnClick;
                SavedMessages.Add(item);
            }
        }

        private void doAutorization()
        {
            MessageBox.Show("נשלח אליך בעוד רגע קוד אימות לנייד, הכנס אותו בחלון הבא.");
            string hash = TLSClient.SendCodeRequestAsync(ClientSingelton.userDetails.PhoneNumber).Result ;
            string code = ShowTextInputWindow();
            TLSClient.MakeAuthAsync(ClientSingelton.userDetails.PhoneNumber, hash, code).Wait();
            if (TLSClient.IsUserAuthorized())
            {
                MessageBox.Show("אימות הצליח!");
            }
            else
            {
                MessageBox.Show("תהליך האימות נכשל, אנא הפעל את התוכנה מחדש");
                Environment.Exit(0);
            }

        }

        public bool isDigitsOnly(string inStr)
        {
            foreach (char c in inStr.Substring(1,inStr.Length - 1))
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }
            return true;
            
        }

        public async void OnSendCmd()
        {
            if (SelectedMsgProperties.dest == null)
            {
                MessageBox.Show("חובה לבחור נמען");
                return;
            }

            if (CurrentMessage.isScheduled)
            {
                string msg = "ההודעה תשלח ל: \n" + selectedDestText + "\n" + " בתאריך : \n" + CurrentMessage.date.ToString();
                MessageBox.Show(msg);
                CurrentMessage.properties = SelectedMsgProperties;
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Send's scheduled telegram messages";
                    
                    td.Triggers.Add(new TimeTrigger { StartBoundary = CurrentMessage.date });

                    string exeName = "MessageScheduler.exe";
                    // TODO: update to user
                    StreamWriter sw = new StreamWriter($"temp_" + CurrentMessage.date.Ticks.ToString().Replace(".", "_") + ".xml");
                    XmlSerializer serializer = new XmlSerializer(typeof(TelegramMessage));
                    serializer.Serialize(sw, CurrentMessage);

                    string pathExe = ((FileStream)sw.BaseStream).Name;
                    if (pathExe.Contains(" "))
                    {
                        pathExe = pathExe.Replace(" ", "*");
                    }
                    string arguments = pathExe;
                    arguments += $",{TelegramUserDetails.Instance.ApiAppID}";
                    arguments += $",{TelegramUserDetails.Instance.ApiHASH}";

                    // close file
                    sw.Close();
                    string dir = Directory.GetCurrentDirectory();
                    
                    td.Actions.Add(new ExecAction(Path.Combine(dir, exeName), arguments, Directory.GetCurrentDirectory()));
                    Microsoft.Win32.TaskScheduler.Task tsk = ts.RootFolder.RegisterTaskDefinition("TLAutomator", td);

                    ts.BeginInit();
                }
                return;
            }

            //await TLSClient.ConnectAsync();
            bool result = false;
            switch (SelectedMsgProperties.msgType)
            {
                //SelectedMsgProperties.dest[0] is group / channel name
                case MsgType.channel:
                    result = await ClientSingelton.SendMessageToGroupChannel(SelectedMsgProperties.dest[0],
                                         false, CurrentMessage);
                    if (!result)
                    {
                        MessageBox.Show("לא ניתן לשלוח הודעה  לערוץ זה.\nישנם ערוצים שלא ניתן לקבל את רשימת המשתמשים בהם (על בסיס הרשמה ולא הצטרפות)");
                    }
                    break;
                case MsgType.group:
                    result = await ClientSingelton.SendMessageToGroupChannel(SelectedMsgProperties.dest[0],
                                         true, CurrentMessage);
                    break;
                case MsgType.users:
                    result = await ClientSingelton.BroadcastMessage(SelectedMsgProperties.dest,
                                        CurrentMessage);
                    break;
                case MsgType.excel:
                    foreach (string dest in SelectedMsgProperties.dest)
                    {
                        if (isDigitsOnly(dest))
                        {
                            try
                            {
                                result = await ClientSingelton.SendMessageToUser(dest, CurrentMessage);
                            }
                            catch (TLSharp.Core.Network.Exceptions.FloodException e)
                            {
                                Thread.Sleep((int)e.TimeToWait.TotalMilliseconds + 10);
                                result = await ClientSingelton.SendMessageToUser(dest, CurrentMessage);
                            }
                        }
                        else
                        {
                            try
                            {
                                result = await ClientSingelton.SendToTelegramUser(dest, CurrentMessage);
                            }
                            catch (TLSharp.Core.Network.Exceptions.FloodException e)
                            {
                                Thread.Sleep((int)e.TimeToWait.TotalMilliseconds + 10);
                                result = await ClientSingelton.SendToTelegramUser(dest, CurrentMessage);
                            }
                        }
                    }
                    break;
                default:
                    return;
            }
            if (result)
                MessageBox.Show("ההודעה נשלחה ל: " + SelectedDestinationText);
            else
            {
                if (SelectedMsgProperties.msgType == MsgType.users)
                    MessageBox.Show("Send Failed to one or more users.Make sure all users are telegram users.");
            }
        }

        private bool CheckCurrentMessage()
        {
            if (CurrentMessage == null || CurrentMessage.Message == string.Empty)
            {
                MessageBox.Show("אין לשלוח הודעה ריקה");
                return false;
            }
            return true;
        }

        private async void OnChoseExcel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                List<string> userNumbers = parseExcel(openFileDialog.InitialDirectory + openFileDialog.FileName);
                if (userNumbers.Count > 0)
                {
                    SelectedMsgProperties.dest = userNumbers;
                    SelectedMsgProperties.msgType = MsgType.excel;
                    CurrentMessage.destChatId = userNumbers;
                    SelectedDestinationText = openFileDialog.FileName;
                }
                else
                    MessageBox.Show("לא נמצא שם בטבלת האקסל, אנא בדוק שהיא תקינה.");

            }
        }


        private void OnChosePrvUser()
        {
            //await client.ConnectAsync();
            string phoneNumber = ShowTextInputWindow();
            if (phoneNumber == null)
                return;
            if (phoneNumber.Length > 0)
            {
                SelectedMsgProperties.dest = new List<string> { phoneNumber };
                SelectedMsgProperties.msgType = MsgType.users;
                SelectedDestinationText = phoneNumber;
                CurrentMessage.destChatId.Add(phoneNumber);
            }
            else
                MessageBox.Show("שם הערוץ/קבוצה לא הוקלד כשורה");
        }

        private async void OnChannelGroupCmd(bool isGroup)
        {
            //   await TLSClient.ConnectAsync();

            //string channelLink = ShowTextInputWindow();
            string selectedChat = await ShowGroupListView(isGroup);
            if (selectedChat == null)
                return;

            if (selectedChat.Length > 0)
            {
                SelectedMsgProperties.dest = new List<string> { selectedChat };
                SelectedMsgProperties.msgType = isGroup ? MsgType.group : MsgType.channel;
                SelectedDestinationText = selectedChat;
                CurrentMessage.destChatId.Add(selectedChat);
            }
            else
                MessageBox.Show("שם הערוץ/קבוצה לא נבחר");
        }

        private async Task<string> ShowGroupListView(bool isGroup)
        {
            try
            {
                List<string> chatList;//= new List<string>();
                var groupChats = await ClientSingelton.GetAllDialogs();
                if (isGroup)
                {
                    chatList = groupChats
                                     .OfType<TLChat>()
                                     .ToList()
                                     .Select(o => o.Title)
                                     .ToList();
                }
                else
                {
                    chatList = groupChats
                                     .OfType<TLChannel>()
                                     .ToList()
                                     .Select(o => o.Title)
                                     .ToList();
                }
                //var listItems = groupChats.Dia
                RemoteUsersList lst = new RemoteUsersList
                {
                    List = new ObservableCollection<string>(chatList)
                };
                ListViewPage listView = new ListViewPage(lst);
                return listView.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("תקלה - " + ex.Message);
                return null;
            }
        }

        private string ShowTextInputWindow()
        {
            return new InputBox("", "", "").ShowDialog();
        }


        //CHANGE - return list of usernames instead of one
        private List<string> parseExcel(string fileName)
        {
            List<string> numbers = new List<string>();
            XSSFWorkbook hssfwb;
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    // load spread sheet
                    hssfwb = new XSSFWorkbook(file);
                    ISheet sheet = hssfwb.GetSheetAt(0);
                    IRow headerRow = sheet.GetRow(0);
                    IEnumerator rows = sheet.GetRowEnumerator();
                    if (headerRow.LastCellNum == 0)
                    {
                        // todo: handle empty/invalid spreadsheet. for now just throw exception.
                        throw new FileLoadException();
                    }

                    // todo fix iterating over rows
                    while (rows.MoveNext())
                    {
                        var currentRow = rows.Current as IRow;
                        for (int i = 0; i < currentRow.LastCellNum; i++)
                        {
                            numbers.Add(currentRow.GetCell(i).ToString());
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("שגיאה בעת פתיחת קובת אקסל. (האם הקובץ פתוח ?)");
            }
            return numbers;


        }

        private void OnDelete()
        {
            TelegramMessage.DeleteMessageFromStorage(SelectedMsg.Content as string);
            existingMessages.Remove(existingMessages.Find(x => x.Name == SelectedMsg.Content as string));
            SavedMessages.Remove(SelectedMsg);
            if (SavedMessages.Count > 0)
                CurrentMessage = existingMessages[0];
            else
                CurrentMessage = null;
        }
        public void ChangeMessageBtnClick(object sender, RoutedEventArgs e)
        {
            CurrentMessage = existingMessages.Find(x => x.Name == (string)((ListViewItem)sender).Content);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void CtrlSaveCmd()
        {
            if (CurrentMessage == null)
                return;
            var save = MessageBox.Show("האם לשמור שינויים?", "Question",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (save == MessageBoxResult.Yes)
            {
                CurrentMessage.isScheduled = false;
                CurrentMessage.date = default;
                CurrentMessage.Save();
                CurrentMessage.properties = SelectedMsgProperties;
                CurrentMessage.isScheduled = SelectedDate == default;
            }
        }

        private async void doTelegramLogin()
        {
            if (TLSClient == null)
                return;

            if (TLSClient.IsUserAuthorized())
                return;

            TLUser user;
            string hash;
            try
            {
                await TLSClient.ConnectAsync();
                hash = await TLSClient.SendCodeRequestAsync(ClientSingelton.userDetails.PhoneNumber);
                //TelegramUserDetails.ApiHASH = hash;
            }
            catch (TLSharp.Core.Network.Exceptions.FloodException e)
            {
                MessageBox.Show("You have to wait: " + e.TimeToWait.ToString());
                return;
            }
            var code = new InputBox("", "", "").ShowDialog();
            if (code.Length > 0)
            {
                user = await TLSClient.MakeAuthAsync(ClientSingelton.userDetails.PhoneNumber, hash, code);
            }

            if (!TLSClient.IsConnected)
            {
                return;
            }


        }

    }
}

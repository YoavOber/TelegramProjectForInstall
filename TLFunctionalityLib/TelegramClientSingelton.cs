using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeleSharp.TL.Users;
using TeleSharp.TL;
using TeleSharp.TL.Channels;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using TLSharp.Core.Utils;
using TeleSharp.TL.Contacts;
using System.Threading;

namespace TLFunctionalityLib
{

    public class TelegramClientSingelton
    {
        public enum destType
        {
            user,
            group,
            channel
        }

        public static TLContacts contacts { get; set; }

        private static TelegramClientSingelton _instance;
        public static TelegramClientSingelton Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TelegramClientSingelton();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        public TelegramClient client { get; set; }
        public TelegramUserDetails userDetails { get; set; }


        public TelegramClientSingelton(SessionStore sessionStore = null, string sessionID = "session")
        {
            userDetails = TelegramUserDetails.Instance;

            client = new TelegramClient(userDetails.ApiAppID, userDetails.ApiHASH);
            client.ConnectAsync().Wait();
        }


        public async Task<bool> BroadcastMessage(List<string> phoneNumbers, TelegramMessage message)
        {
            bool result = true;

            foreach (var p in phoneNumbers)
            {
                //await ? Elay: of course !!! (or use a task pool and await all of them.)

                result &= await SendMessageToUser(p, message);
            }
            return result;
        }


        public async Task SendFile(MediaType type, string path, string fileName, TLChat chat, string mimeType)
        {
            // type : 1 - image, 0 - document
            var fileResult = await client.UploadFile(fileName, new StreamReader(path));
            if (type == MediaType.image)
            {
                // send image
                await client.SendUploadedPhoto(new TLInputPeerUser() { UserId = chat.Id }, fileResult, "");
            }
            else
            {
                // send another file
                await client.SendUploadedDocument(
                    new TLInputPeerUser() { UserId = chat.Id },
                    fileResult,
                    "",
                    mimeType,
                    new TLVector<TLAbsDocumentAttribute>());
            }
        }



        public async Task<bool> SendMessageToUser(string phoneNumber, TelegramMessage message)
        {

            if (phoneNumber.Length < 7)
            {
                //throw new ArgumentException("Invalid phone number");
                return false;
            }
            if (phoneNumber == null || message == default)
                // no username set
                return false;


            bool isBrazilian = false;

            string globalPhoneNumber;

            if (contacts == null)
            {
                contacts = await client.GetContactsAsync();

            }


            if (contacts.Users.Count < 1)
                // user not found
                return false;

            if ((phoneNumber.Substring(0,2) == "55" || phoneNumber.Substring(0,3) == "+55") && phoneNumber.Length > 8)
            {
                // Brazlian phone number
                globalPhoneNumber = phoneNumber;
                isBrazilian = true;
            }

            // skip the first 3 digits (052, 053, ...etc) -> this is for israeli phone number
            else if (phoneNumber[0] == '0')
            {
                globalPhoneNumber = "972" + phoneNumber.Substring(1, phoneNumber.Length - 1);
                phoneNumber = phoneNumber.Substring(3);

            }
            else if(phoneNumber.Substring(0, 3) == "972")
            {
                globalPhoneNumber = phoneNumber;
            }
            else if (phoneNumber.Substring(4) == "+972")
            {
                globalPhoneNumber = phoneNumber.Substring(1, phoneNumber.Length - 1);
                phoneNumber = phoneNumber.Substring(4);
            }
            else
            {
                globalPhoneNumber = "972" + phoneNumber;
                phoneNumber = phoneNumber.Substring(2);
            }


            var targetUser = new TLUser();
            if (!isBrazilian)
                targetUser.Phone = "972" + phoneNumber;
            else
                targetUser.Phone = "55" + phoneNumber;


            
            var contact = contacts.Users
                .Where(x => x.GetType() == typeof(TLUser))
                .Cast<TLUser>()
                .FirstOrDefault(o => o.Phone != null &&  o.Phone.Length > 5 && o.Phone.Substring(5) == phoneNumber);

            if (contact != null)
            {
                return await SendPrivateMessage(contact, message);
            }
            else
            {
                // check if the contact is new
                contacts = await client.GetContactsAsync();
                contact = contacts.Users
                                  .Where(x => x.GetType() == typeof(TLUser))
                                  .Cast<TLUser>()
                                  .FirstOrDefault(o => o.Phone != null && o.Phone.Length > 5 && o.Phone.Substring(5) == phoneNumber);
                
                if (contact != null)
                {
                    return await SendPrivateMessage(contact, message);
                }
            }

            TLRequestImportContacts requestImportContacts = new TLRequestImportContacts();

            requestImportContacts.Contacts = new TLVector<TLInputPhoneContact>();
            requestImportContacts.Contacts.Add(new TLInputPhoneContact()
            {
                Phone = globalPhoneNumber,
                FirstName = "איש קשר חדש",
                LastName = ""
            }); 

            var userList = await client.SendRequestAsync<TLImportedContacts>(requestImportContacts);
            var user = userList.Users.Cast<TLUser>().ToList().FirstOrDefault();

            if (user == null)
                return false;

            targetUser.AccessHash = user.AccessHash;
            targetUser.Id = user.Id;
            return await SendPrivateMessage(targetUser, message);
        }

        public async Task<bool> SendToTelegramUser(string userName, TelegramMessage message)
        {
            TLUser contact;
            if (contacts != null)
            {
                contact = contacts.Users
                                  .Where(x => x.GetType() == typeof(TLUser))
                                  .Cast<TLUser>()
                                  .FirstOrDefault(o => o.Username != null && o.Username.ToLower().Equals(userName.ToLower()));

                if (contact != null)
                {
                    return await SendPrivateMessage(contact, message);
                }
                else
                {
                    // check if the contact is new
                    contacts = await client.GetContactsAsync();
                    contact = contacts.Users
                                       .Where(x => x.GetType() == typeof(TLUser))
                                       .Cast<TLUser>()
                                       .FirstOrDefault(o => o.Username != null && o.Username.ToLower().Equals(userName.ToLower()));

                    if (contact != null)
                    {
                        return await SendPrivateMessage(contact, message);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                contacts = await client.GetContactsAsync();
                contact = contacts.Users
                                   .Where(x => x.GetType() == typeof(TLUser))
                                   .Cast<TLUser>()
                                   .FirstOrDefault(o => o.Username != null && o.Username.ToLower().Equals(userName.ToLower()));

                if (contact != null)
                {
                    return await SendPrivateMessage(contact, message);
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> SendMessageToGroupChannel(string selectedChat, bool group, TelegramMessage message)
        {

            if (selectedChat == null)
                return false;

            bool result = true;

            if (group)
            {
                var ls = await GetGroupUserList(selectedChat);
                foreach (var user in ls)
                {
                    result &= await SendPrivateMessage(user, message);
                }
            }
            else
            {

                // send message to each channel participant
                List<TLUser> ls;
                try
                {
                    ls = await GetChannelUserList(selectedChat);

                }
                catch
                {
                    return false;
                }

                foreach (var user in ls)
                {
                    result &= await SendPrivateMessage(user, message);
                }
                /*
                var AllChats = await GetAllDialogs();
                var channelChat = AllChats
                     .OfType<TLChannel>()
                     .FirstOrDefault(c => c.Title == selectedChat);

                result = await SendToChannel(channelChat, message);
                */
            }
            return result;
        }

        public async Task<TLVector<TLAbsChat>> GetAllDialogs()
        {
            await Connect();
            var chatTsk = (TLDialogs) await client.GetUserDialogsAsync();
            //return x.Chats;
            return chatTsk.Chats;
        }

        public async Task<bool> SendPrivateMessage(TLUser myContact, TelegramMessage message)
        {

            if (!client.IsConnected)
                await Connect();
            try
            {
                // send media
                if (message.mediaType != MediaType.Non)
                {
                    var chat = new TLChat() { Id = myContact.Id };
                    await SendFile(message.mediaType, message.MediaPath, message.fileName, chat, message.MimeType);
                }

                // send message
                await client.SendMessageAsync(new TLInputPeerUser()
                {
                    UserId = myContact.Id,
                    AccessHash = (long)myContact.AccessHash
                }, message.Message);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task<bool> SendToChannel(TLChannel channelChat, TelegramMessage message)
        {
            try
            {
                if (message.mediaType != MediaType.Non)
                {
                    var chat = new TLChat() { Id = channelChat.Id };

                    await SendFile(message.mediaType, message.MediaPath, message.fileName, chat, message.MimeType);
                }
                await client.SendMessageAsync(new TLInputPeerChannel()
                {
                    ChannelId = channelChat.Id,
                    AccessHash = channelChat.AccessHash.Value
                }, message.Message);
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }


        private async Task Connect()
        {
            if (client == null)
                client = new TelegramClient(userDetails.ApiAppID, userDetails.ApiHASH);
            await client.ConnectAsync();
        }

        public async Task<IEnumerable<TLUser>> GetGroupUserList(string groupName)
        {
            var dialogs = await GetAllDialogs();
            var group = dialogs
                     .OfType<TLChat>()
                     .FirstOrDefault(c => c.Title == groupName);
            var req = new TLRequestGetFullChat { ChatId = group.Id };
            var res = await client.SendRequestAsync<TeleSharp.TL.Messages.TLChatFull>(req);
            //client.SendRequestAsync<TeleSharp.TL.Messages.TLRequestGetFullChat>(req);
            return res.Users.Cast<TLUser>();
        }

        public async Task<List<TLUser>> GetChannelUserList(string channelName)
        {
            var dialogs = await GetAllDialogs();
            var channel = dialogs
                     .OfType<TLChannel>()
                     .FirstOrDefault(c => c.Title == channelName);

            var absChannel = channel.AccessHash;

            TLInputChannel InputChannel = new TLInputChannel { ChannelId = channel.Id, AccessHash = channel.AccessHash.Value };




            int offset = 200;
            var req = new TLRequestGetParticipants { Channel = InputChannel ,
                Filter = new TLChannelParticipantsRecent() { },
                Offset = 0,
                Limit = 200
            };

            var res = await client.SendRequestAsync<TLChannelParticipants>(req);
            List<TLUser> users = new List<TLUser>();

            users.AddRange(res.Users.ToList().Cast<TLUser>());
            //users.Concat(res.Users);

            int userCnt = res.Count - 200;
            int remainingUsers = userCnt;
            while (remainingUsers > 0)
            {
                req = new TLRequestGetParticipants
                {
                    Channel = InputChannel,
                    Filter = new TLChannelParticipantsRecent() { },
                    Offset = offset,
                    Limit = 200
                };
                users.AddRange(res.Users.ToList().Cast<TLUser>());
                offset += res.Users.Count;
                remainingUsers -= res.Users.Count;
            }


            return users;
        }
    }
}